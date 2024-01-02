using System.Net;
using Adramelech.Http.Extensions;
using Adramelech.Utilities;
using Serilog;

namespace Adramelech.Http.Server;

public abstract class MiddlewareBase
{
    protected HttpListenerContext Context = null!;
    protected HttpListenerRequest Request = null!;
    protected EndpointBase Endpoint = null!;

    public async Task<bool> HandleRequestAsync(HttpListenerContext context, HttpListenerRequest request,
        EndpointBase endpoint)
    {
        Context = context;
        Request = request;
        Endpoint = endpoint;

        var result = await ExceptionUtils.TryAsync(HandleRequestAsync);

        if (!result.IsFailure) return result.Value;

        Log.Error(result.Exception, "Failed to handle request");
        await context.RespondAsync("Failed to handle request", HttpStatusCode.InternalServerError);
        return false;
    }

    protected abstract Task<bool> HandleRequestAsync();
}