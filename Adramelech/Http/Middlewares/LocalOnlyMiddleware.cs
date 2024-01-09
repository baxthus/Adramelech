using System.Net;
using System.Text.RegularExpressions;
using Adramelech.Http.Server;
using Adramelech.Http.Utilities;

namespace Adramelech.Http.Middlewares;

public class LocalOnlyMiddleware : MiddlewareBase
{
    public override Regex Path => LocalOnlyMiddlewareHelper.PathRegex();

    protected override async Task<bool> HandleAsync()
    {
        if (Context.Request.IsLocal) return true;

        await Context.RespondAsync("This endpoint is only available locally", HttpStatusCode.Forbidden);
        return false;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal partial class LocalOnlyMiddlewareHelper
{
    [GeneratedRegex(@"^/files/?$")]
    public static partial Regex PathRegex();
}