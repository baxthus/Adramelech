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

        FileSchema file;
        try
        {
            file = await DatabaseManager.Files.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception e)
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

            try
            {
                await DatabaseManager.Files.DeleteOneAsync(filter);
            }
            catch
            {
                // ignored
            }

            return;
        }

        var chunks = new List<byte[]>();

        foreach (var message in messages)
        {
            var (chunk, failed) = await DownloadPart(message);
            if (failed)
            {
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

    private static async Task<(byte[]?, bool)> DownloadPart(IMessage message)
    {
        var attachment = message.Attachments.FirstOrDefault();
        if (attachment is null)
            return (null, true);

        var response = await attachment.Url.Request<byte[]>();
        return response is null ? (null, true) : (response, false);
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