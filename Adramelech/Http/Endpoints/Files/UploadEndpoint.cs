using System.Net;
using Adramelech.Database;
using Adramelech.Http.Attributes;
using Adramelech.Http.Common;
using Adramelech.Http.Extensions;
using Adramelech.Http.Schemas;
using Adramelech.Http.Utilities;
using Adramelech.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Adramelech.Http.Endpoints.Files;

[Endpoint("/files/upload", "POST")]
[NeedsToken]
public class UploadEndpoint : EndpointBase
{
    protected override async Task HandleAsync()
    {
        var result = ExceptionUtils.Try(GetBody);
        if (result.IsFailure)
        {
            if (result.Exception is InvalidOperationException)
            {
                await Context.RespondAsync("Missing body", HttpStatusCode.BadRequest);
                return;
            }

            Log.Error(result.Exception, "Failed to get body");
            await Context.RespondAsync("Failed to get body", HttpStatusCode.InternalServerError);
            return;
        }

        var body = result.Value;

        var channel = await FilesEndpointUtils.GetChannel(BotClient);
        if (channel is null)
        {
            await Context.RespondAsync("Channel not found", HttpStatusCode.InternalServerError);
            return;
        }

        var chunks = new List<byte[]>();

        const int chunkSize = 8 * 1024 * 1024; // 8Mb
        for (var i = 0; i < body!.Length; i += chunkSize)
        {
            var chunk = body[i..Math.Min(i + chunkSize, body.Length)];
            chunks.Add(chunk);
        }

        var file = new FileSchema
        {
            Id = ObjectId.GenerateNewId(),
            CreatedAt = ObjectId.GenerateNewId().CreationTime,
            Available = false,
            FileName = Request.QueryString["fileName"]?.Split(".")[0],
            ContentType = Request.Headers["Content-Type"] ?? "application/octet-stream",
            TotalChunks = chunks.Count,
            Chunks = []
        };

        var insert = await ExceptionUtils.TryAsync(() => DatabaseManager.Files.InsertOneAsync(file));
        if (insert.IsFailure)
        {
            Log.Error(insert.Exception, "Failed to insert file into database");
            await Context.RespondAsync("Failed to insert file into database", HttpStatusCode.InternalServerError);

            return;
        }

        await Context.RespondAsync(file.ToJson(new CamelCaseNamingStrategy()), contentType: "application/json");

        foreach (var chunk in chunks)
        {
            var chunkInfo = new FileChunkSchema
            {
                CurrentChunk = chunks.IndexOf(chunk) + 1
            };

            var content = new MessageSchema
            {
                Id = file.Id,
                TotalChunks = file.TotalChunks,
                CurrentChunk = chunkInfo.CurrentChunk
            };

            var message = await ExceptionUtils.TryAsync(() =>
                channel.SendFileAsync(new MemoryStream(chunk), file.FileName ?? "file",
                    content.ToJson(new KebabCaseNamingStrategy())));
            if (message.IsFailure)
            {
                Log.Error(message.Exception, "Failed to send file chunk");
                await Context.RespondAsync("Failed to send file chunk", HttpStatusCode.InternalServerError);

                return;
            }

            chunkInfo.MessageId = message.Value!.Id;
            file.Chunks.Add(chunkInfo);
        }

        file.Available = true;

        var filter = Builders<FileSchema>.Filter.Eq(x => x.Id, file.Id);
        var update = await ExceptionUtils.TryAsync(() => DatabaseManager.Files.ReplaceOneAsync(filter, file));
        if (update.IsFailure)
        {
            Log.Error(update.Exception, "Failed to update file");
            await Context.RespondAsync("Failed to update file", HttpStatusCode.InternalServerError);

            foreach (var message in file.Chunks)
                await channel.DeleteMessageAsync(message.MessageId);
        }
    }

    private struct MessageSchema
    {
        public ObjectId Id { get; set; }
        public int TotalChunks { get; set; }
        public int CurrentChunk { get; set; }
    }
}