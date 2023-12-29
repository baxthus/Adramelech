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
using Serilog;

namespace Adramelech.Http.Endpoints.Files;

[Endpoint("/files/delete", "DELETE")]
[NeedsToken]
public class DeleteEndpoint : EndpointBase
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

        if (!ObjectId.TryParse(id, out var objectId))
        {
            await Context.RespondAsync("Invalid id parameter", HttpStatusCode.BadRequest);
            return;
        }

        var filter = Builders<FileSchema>.Filter.Eq(x => x.Id, objectId);

        var file = await ExceptionUtils.TryAsync(() => DatabaseManager.Files.Find(filter).FirstOrDefaultAsync());
        if (file.IsFailure)
        {
            Log.Error(file.Exception, "Failed to query database");
            await Context.RespondAsync("Failed to query database", HttpStatusCode.InternalServerError);
            return;
        }

        if (file.Value.IsDefault())
        {
            await Context.RespondAsync("File not found", HttpStatusCode.NotFound);
            return;
        }

        var result = await ExceptionUtils.TryAsync(() => DatabaseManager.Files.DeleteOneAsync(filter));
        if (result.IsFailure)
        {
            Log.Error(result.Exception, "Failed to delete file");
            await Context.RespondAsync("Failed to delete file", HttpStatusCode.InternalServerError);
            return;
        }

        await Context.RespondAsync("File deleted");

        var (messages, _) = await channel.GetAllMessages(file.Value.Chunks.Select(x => x.MessageId));

        // Delete after response, because if the deletion fails we can't do nothing about it anyway
        await channel.DeleteMessagesAsync(messages);
    }
}