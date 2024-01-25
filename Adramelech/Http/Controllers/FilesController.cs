using System.Diagnostics.CodeAnalysis;
using System.Net;
using Adramelech.Http.Attributes;
using Adramelech.Http.Attributes.Http;
using Adramelech.Http.Attributes.Middleware;
using Adramelech.Http.Server;
using Adramelech.Models;
using Adramelech.Services;
using Adramelech.Utilities;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Adramelech.Http.Controllers;

[ApiController]
[Route("files")]
[NeedsToken]
public class FilesController : ControllerBase
{
    private DatabaseService _dbService = null!;
    private ConfigService _configService = null!;
    private DiscordSocketClient _botClient = null!;

    protected override Task InitializeAsync()
    {
        _dbService = Provider.GetRequiredService<DatabaseService>();
        _configService = Provider.GetRequiredService<ConfigService>();
        _botClient = Provider.GetRequiredService<DiscordSocketClient>();
        return Task.CompletedTask;
    }

    [HttpGet("list")]
    public async Task ListAsync()
    {
        var result = await ErrorUtils.TryAsync(() => _dbService.Files.Get());
        if (result.IsFailure)
        {
            await RespondAsync(false, "Failed to get files from database", HttpStatusCode.InternalServerError);
            return;
        }

        if (result.Value!.Models.Count == 0)
        {
            await RespondAsync(false, "No files found");
            return;
        }

        await RespondAsync(true, result.Value.Models);
    }

    [HttpPost("upload")]
    public async Task UploadAsync()
    {
        var result = ErrorUtils.Try(GetBody);
        if (result.IsFailure)
        {
            if (result.Exception?.Message == "No body found")
            {
                await RespondAsync(false, "Missing body", HttpStatusCode.BadRequest);
                return;
            }

            await RespondAsync(false, "Failed to process body", HttpStatusCode.UnprocessableContent);
            return;
        }

        var body = result.Value!;

        var channel = await GetChannel(_configService.FilesChannel);
        if (channel is null)
        {
            await RespondAsync(false, "Files channel not found", HttpStatusCode.InternalServerError);
            return;
        }

        var chunks = new List<byte[]>();

        const int chunkSize = 1024 * 1024 * 8; // 8Mb

        for (var i = 0; i < body.Length; i += chunkSize)
        {
            var chunk = body.Skip(i).Take(chunkSize).ToArray();
            chunks.Add(chunk);
        }

        var file = new FileModel
        {
            CreatedAt = DateTime.UtcNow,
            Available = false,
            FileName = Request.QueryString["fileName"]?.Split(".")[0],
            ContentType = Request.Headers["Content-Type"] ?? "application/octet-stream",
            TotalChunks = chunks.Count,
            Chunks = [],
        };

        var insert = await ErrorUtils.TryAsync(() => _dbService.Files.Insert(file));
        if (insert.IsFailure)
        {
            Log.Error(insert.Exception, "Failed to insert file into database");
            await RespondAsync(false, "Failed to insert file into database", HttpStatusCode.InternalServerError);
            return;
        }

        file = insert.Value?.Model!;

        await RespondAsync(true, file, HttpStatusCode.Created);

        foreach (var chunk in chunks)
        {
            var chunkInfo = new FileChunkModel
            {
                CurrentChunk = chunks.IndexOf(chunk) + 1,
            };

            var content = new MessageModel(file.Id, file.TotalChunks, chunkInfo.CurrentChunk);

            var message = await ErrorUtils.TryAsync(() =>
                channel.SendFileAsync(new MemoryStream(chunk), file.FileName ?? "file", content.ToJson()));
            if (message.IsFailure)
            {
                Log.Error(message.Exception, "Failed to send file chunk");

                await ErrorUtils.TryAsync(() => file.Delete<FileModel>());
                await CleanupFiles(channel, file.Chunks.Select(x => x.MessageId));

                return;
            }

            chunkInfo.MessageId = message.Value!.Id;
            file.Chunks.Add(chunkInfo);
        }

        file.Available = true;

        var update = await ErrorUtils.TryAsync(() => file.Update<FileModel>());
        if (update.IsFailure)
        {
            Log.Error(update.Exception, "Failed to update file in database");

            await ErrorUtils.TryAsync(() => file.Delete<FileModel>());
            await CleanupFiles(channel, file.Chunks.Select(x => x.MessageId));
        }
    }

    [HttpGet("download")]
    public async Task DownloadAsync()
    {
        var guid = await GetId();
        if (guid is null)
            return;

        var channel = await GetChannel(_configService.FilesChannel);
        if (channel is null)
        {
            await RespondAsync(false, "Files channel not found", HttpStatusCode.InternalServerError);
            return;
        }

        var file = await ErrorUtils.TryAsync(() => _dbService.Files.Where(x => x.Id == guid).Single());
        if (file.IsFailure)
        {
            Log.Error(file.Exception, "Failed to get file from database");
            await RespondAsync(false, "Failed to get file from database", HttpStatusCode.InternalServerError);
            return;
        }

        if (file.Value.IsDefault())
        {
            await RespondAsync(false, "File not found");
            return;
        }

        if (!file.Value!.Available)
        {
            await RespondAsync(false, "File is not available", HttpStatusCode.Forbidden);
            return;
        }

        var (messages, missing) = await GetAllMessages(channel, file.Value.Chunks.Select(x => x.MessageId));
        if (missing)
        {
            await RespondAsync(false, "File is missing chunks", HttpStatusCode.InternalServerError);

            await ErrorUtils.TryAsync(() => file.Value.Delete<FileModel>());
            await CleanupFiles(channel, file.Value.Chunks.Select(x => x.MessageId));

            return;
        }

        var chunks = new List<byte[]>();

        foreach (var message in messages)
        {
            var chunk = await ErrorUtils.TryAsync(() => DownloadPart(message));
            if (chunk.IsFailure)
            {
                Log.Error(chunk.Exception, "Failed to download file chunk");
                await RespondAsync(false, "Failed to download one or more chunks", HttpStatusCode.InternalServerError);
                return;
            }

            chunks.Add(chunk.Value!);
        }

        if (file.Value.TotalChunks != chunks.Count)
        {
            await RespondAsync(false, "One or more chunks are missing", HttpStatusCode.InternalServerError);
            return;
        }

        var buffer = CombineBytes(chunks);

        await RespondAsync(buffer, contentType: file.Value.ContentType);
    }

    [HttpDelete("delete")]
    public async Task DeleteAsync()
    {
        var guid = await GetId();
        if (guid is null)
            return;

        var channel = await GetChannel(_configService.FilesChannel);
        if (channel is null)
        {
            await RespondAsync(false, "Files channel not found", HttpStatusCode.InternalServerError);
            return;
        }

        var file = await ErrorUtils.TryAsync(() => _dbService.Files.Where(x => x.Id == guid.Value).Single());
        if (file.IsFailure)
        {
            Log.Error(file.Exception, "Failed to get file from database");
            await RespondAsync(false, "Failed to get file from database", HttpStatusCode.InternalServerError);
            return;
        }

        if (file.Value.IsDefault())
        {
            await RespondAsync(false, "File not found");
            return;
        }

        var result = await ErrorUtils.TryAsync(() => file.Value!.Delete<FileModel>());
        if (result.IsFailure)
        {
            Log.Error(result.Exception, "Failed to delete file from database");
            await RespondAsync(false, "Failed to delete file from database", HttpStatusCode.InternalServerError);
            return;
        }

        await RespondAsync(true, "File deleted successfully");

        await CleanupFiles(channel, file.Value!.Chunks.Select(x => x.MessageId));
    }

    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")]
    private record MessageModel(Guid Id, int TotalChunks, int CurrentChunk);

    private async Task<Guid?> GetId()
    {
        var id = Request.QueryString["id"];
        if (string.IsNullOrEmpty(id))
        {
            await RespondAsync(false, "Missing id", HttpStatusCode.BadRequest);
            return null;
        }

        if (Guid.TryParse(id, out var guid)) return guid;

        await RespondAsync(false, "Invalid id", HttpStatusCode.UnprocessableContent);
        return null;
    }

    private async Task<SocketTextChannel?> GetChannel(ulong? channelId)
    {
        if (channelId is null)
            return null;
        var channel = await _botClient.GetChannelAsync(channelId.Value);
        return channel as SocketTextChannel;
    }

    private static async Task<(List<IMessage>, bool)> GetAllMessages(SocketTextChannel channel, IEnumerable<ulong> ids)
    {
        var messages = new List<IMessage>();
        var missing = false;

        foreach (var id in ids)
        {
            var message = await channel.GetMessageAsync(id);
            if (message is null)
            {
                missing = true;
                continue;
            }

            messages.Add(message);
        }

        return (messages, missing);
    }

    private static async Task<byte[]> DownloadPart(IMessage message)
    {
        var attachment = message.Attachments.FirstOrDefault();
        if (attachment is null)
            throw new Exception("Message has no attachments");

        var response = await attachment.Url.Request<byte[]>();
        return response ?? throw new Exception("Failed to download attachment");
    }

    private static byte[] CombineBytes(List<byte[]> bytes)
    {
        var result = new byte[bytes.Sum(x => x.Length)];
        var offset = 0;
        foreach (var array in bytes)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }

    private static async Task CleanupFiles(ITextChannel channel, IEnumerable<ulong> messages) =>
        await ErrorUtils.TryAsync(() => channel.DeleteMessagesAsync(messages));
}