using System.Net;
using Adramelech.Database;
using Adramelech.Http.Attributes;
using Adramelech.Http.Common;
using Adramelech.Http.Extensions;
using Adramelech.Http.Schemas;
using Adramelech.Http.Utilities;
using Adramelech.Utilities;
using MongoDB.Bson;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Adramelech.Http.Endpoints.Files;

[Endpoint("/files/upload", "POST")]
[NeedsToken]
public class UploadEndpoint : EndpointBase
{
    protected override async Task HandleAsync()
    {
        var body = GetBody();
        if (body is null)
        {
            await Context.RespondAsync("Missing body", HttpStatusCode.BadRequest);
            return;
        }

        var channel = await FilesEndpointUtils.GetChannel(BotClient);
        if (channel is null)
        {
            await Context.RespondAsync("Channel not found", HttpStatusCode.InternalServerError);
            return;
        }

        var chunks = new List<byte[]>();

        // Separate in 8mb chunks
        const int chunkSize = 8 * 1024 * 1024;
        for (var i = 0; i < body.Length; i += chunkSize)
        {
            var chunk = body[i..Math.Min(i + chunkSize, body.Length)];
            chunks.Add(chunk);
        }

        var file = new FileSchema
        {
            Id = ObjectId.GenerateNewId(),
            CreatedAt = ObjectId.GenerateNewId().CreationTime,
            FileName = Request.QueryString["fileName"]?.Split(".")[0],
            ContentType = Request.Headers["Content-Type"] ?? "application/octet-stream",
            TotalChunks = chunks.Count,
            Chunks = []
        };

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

            var message = await channel.SendFileAsync(new MemoryStream(chunk), file.FileName ?? "file",
                content.ToJson(new KebabCaseNamingStrategy()));
            chunkInfo.MessageId = message.Id;
            file.Chunks.Add(chunkInfo);
        }

        try
        {
            await DatabaseManager.Files.InsertOneAsync(file);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to insert file into database");
            await Context.RespondAsync("Failed to insert file into database", HttpStatusCode.InternalServerError);

            foreach (var message in file.Chunks)
                await channel.DeleteMessageAsync(message.MessageId);
        }

        await Context.RespondAsync(file.ToJson(new CamelCaseNamingStrategy()), contentType: "application/json");
    }

    private struct MessageSchema
    {
        public ObjectId Id { get; set; }
        public int TotalChunks { get; set; }
        public int CurrentChunk { get; set; }
    }
}