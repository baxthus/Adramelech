using System.Net;
using System.Text.RegularExpressions;
using Adramelech.Http.Utilities;
using Adramelech.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Adramelech.Http.Server;

public abstract class MiddlewareBase
{
    protected HttpListenerContext Context = null!;
    protected HttpListenerRequest Request = null!;
    protected ControllerBase Controller = null!;
    protected ServiceProvider Provider = null!;

    public virtual Regex? Path => null;

    public async Task<bool> HandleRequestAsync(HttpListenerContext context, HttpListenerRequest request,
        ControllerBase controller, ServiceProvider provider)
    {
        Context = context;
        Request = request;
        Controller = controller;
        Provider = provider;

        var result = await ErrorUtils.TryAsync(HandleAsync);

        if (!result.IsFailure) return result.Value;

        Log.Error(result.Exception, "Failed to handle request");
        await Context.RespondAsync("Failed to handle request", HttpStatusCode.InternalServerError);
        return false;
    }

    protected abstract Task<bool> HandleAsync();
}