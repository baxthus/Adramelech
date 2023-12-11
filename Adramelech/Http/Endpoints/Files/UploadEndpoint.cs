using System.Net;
using Adramelech.Http.Attributes;
using Adramelech.Http.Common;
using Adramelech.Http.Utilities;
using Adramelech.Utilities;
using MongoDB.Bson;

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
            await RespondAsync("Missing body", HttpStatusCode.BadRequest);
            return;
        }

        var channel = await FilesEndpointUtils.GetChannel(BotClient);
        if (channel is null)
        {
            await RespondAsync("Channel not found", HttpStatusCode.InternalServerError);
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

        var ids = new List<ulong>();

        var fileId = ObjectId.GenerateNewId();
        var fileName = Request.QueryString["fileName"]?.Split(".")[0];
        var totalChunks = chunks.Count;

        foreach (var chunk in chunks)
        {
            var fileInfo = new File
            {
                Id = fileId,
                CreatedAt = fileId.CreationTime,
                FileName = fileName,
                TotalChunks = totalChunks,
                CurrentChunk = chunks.IndexOf(chunk) + 1
            };

            var message =
                await channel.SendFileAsync(new MemoryStream(chunk), $"file", JsonUtils.ToJson(fileInfo));
            ids.Add(message.Id);
        }

        await RespondAsync(JsonUtils.ToJson(ids), contentType: "application/json");
    }
}

internal struct File
{
    public ObjectId Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FileName { get; set; }
    public int TotalChunks { get; set; }
    public int CurrentChunk { get; set; }
}