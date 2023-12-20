﻿using System.Net;
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

        var (messages, _) = await channel.GetAllMessages(file.Chunks.Select(x => x.MessageId));

        var tasks = messages.Select(x => x.DeleteAsync());
        await Task.WhenAll(tasks);

        try
        {
            await DatabaseManager.Files.DeleteOneAsync(filter);
        }
        catch
        {
            await Context.RespondAsync("Failed to delete file", HttpStatusCode.InternalServerError);
            return;
        }

        await Context.RespondAsync("File deleted");
    }
}