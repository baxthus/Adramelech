using System.Net;
using Adramelech.Database;
using Adramelech.Http.Attributes;
using Adramelech.Http.Common;
using Adramelech.Http.Extensions;
using Adramelech.Http.Schemas;
using Adramelech.Utilities;
using MongoDB.Driver;
using Newtonsoft.Json.Serialization;

namespace Adramelech.Http.Endpoints.Files;

[Endpoint("/files/list")]
[NeedsToken]
public class ListEndpoint : EndpointBase
{
    protected override async Task HandleAsync()
    {
        List<FileSchema> files;
        try
        {
            files = await DatabaseManager.Files.Find(_ => true).ToListAsync();
        }
        catch
        {
            await Context.RespondAsync("Failed to query database", HttpStatusCode.InternalServerError);
            return;
        }

        if (files.Count == 0)
        {
            await Context.RespondAsync("No files found", HttpStatusCode.NotFound);
            return;
        }

        await Context.RespondAsync(files.ToJson(new CamelCaseNamingStrategy()), contentType: "application/json");
    }
}