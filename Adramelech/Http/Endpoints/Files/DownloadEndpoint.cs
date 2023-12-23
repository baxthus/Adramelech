using System.Net;
using Adramelech.Database;
using Adramelech.Http.Attributes;
using Adramelech.Http.Common;
using Adramelech.Http.Extensions;
using Adramelech.Http.Schemas;
using Adramelech.Http.Utilities;
using Adramelech.Utilities;
using Discord;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace Adramelech.Http.Endpoints.Files;

[Endpoint("/files/download")]
[NeedsToken]
public class DownloadEndpoint : EndpointBase
{
    protected override async Task HandleAsync()
    {
        var id = Request.QueryString["id"];
        if (string.IsNullOrEmpty(id))
        {
            await Context.RespondAsync("Missing id parameter", HttpStatusCode.BadRequest);
            return;
        }

        var channel = await FilesEndpointUtils.GetChannel(BotClient);
        if (channel is null)
        {
            await Context.RespondAsync("Channel not found", HttpStatusCode.InternalServerError);
            return;
        }

        var filter = Builders<FileSchema>.Filter.Eq(x => x.Id, ObjectId.Parse(id));

        var (file, e) =
            await ExceptionUtils.TryAsync(() => DatabaseManager.Files.Find(filter).FirstOrDefaultAsync());
        if (e is not null)
        {
            Log.Error(e, "Failed to query database");
            await Context.RespondAsync("Failed to query database", HttpStatusCode.InternalServerError);
            return;
        }

        if (file.IsDefault())
        {
            await Context.RespondAsync("File not found", HttpStatusCode.NotFound);
            return;
        }

        var (messages, missing) = await channel.GetAllMessages(file.Chunks.Select(x => x.MessageId));
        if (missing)
        {
            await Context.RespondAsync("One or more chunks are missing", HttpStatusCode.NotFound);

            await ExceptionUtils.TryAsync(() => DatabaseManager.Files.DeleteOneAsync(filter));

            return;
        }

        var chunks = new List<byte[]>();

        foreach (var message in messages)
        {
            var (chunk, ex) = await ExceptionUtils.TryAsync(() => DownloadPart(message));
            if (ex is not null)
            {
                Log.Error(ex, "Failed to download chunk");
                await Context.RespondAsync("Failed to download one or more chunks", HttpStatusCode.BadGateway);
                return;
            }

            chunks.Add(chunk!);
        }

        if (file.TotalChunks != chunks.Count)
        {
            await Context.RespondAsync("One or more chunks are missing", HttpStatusCode.NotFound);
            return;
        }

        var buffer = CombineBytes(chunks);

        await Context.RespondAsync(buffer, contentType: file.ContentType);
    }

    private static async Task<byte[]> DownloadPart(IMessage message)
    {
        var attachment = message.Attachments.FirstOrDefault();
        if (attachment is null)
            throw new InvalidOperationException("Message has no attachments");

        var response = await attachment.Url.Request<byte[]>();
        return response ?? throw new InvalidOperationException("Failed to download attachment");
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
}