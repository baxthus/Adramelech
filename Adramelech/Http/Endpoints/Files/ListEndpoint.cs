using System.Net;
using Adramelech.Database;
using Adramelech.Http.Attributes;
using Adramelech.Http.Common;
using Adramelech.Http.Extensions;
using Adramelech.Utilities;
using MongoDB.Driver;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Adramelech.Http.Endpoints.Files;

[Endpoint("/files/list")]
[NeedsToken]
public class ListEndpoint : EndpointBase
{
    protected override async Task HandleAsync()
    {
        var files = await ExceptionUtils.TryAsync(() => DatabaseManager.Files.Find(_ => true).ToListAsync());
        if (files.IsFailure)
        {
            Log.Error(files.Exception, "Failed to query database");
            await Context.RespondAsync("Failed to query database", HttpStatusCode.InternalServerError);
            return;
        }

        if (files.Value!.Count == 0)
        {
            await Context.RespondAsync("No files found", HttpStatusCode.NotFound);
            return;
        }

        await Context.RespondAsync(files.Value.ToJson(new CamelCaseNamingStrategy()), contentType: "application/json");
    }
}