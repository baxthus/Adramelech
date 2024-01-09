using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using Adramelech.Http.Attributes;
using Adramelech.Http.Utilities;
using Adramelech.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Adramelech.Http.Server;

public class ControllerBase
{
    protected HttpListenerContext Context = null!;
    protected HttpListenerRequest Request = null!;
    protected ServiceProvider Provider = null!;
    protected RouteData? CurrentRoute;

    private readonly List<RouteData> _routes;

    public IEnumerable<string> Paths => _routes.Select(r => r.FullPath);

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    protected ControllerBase()
    {
        if (ReflectionUtils.GetAttributes<ApiControllerAttribute>(this).FirstOrDefault() is null)
            throw new InvalidOperationException("Controller attribute not found");

        if (ReflectionUtils.GetAttributes<RouteAttribute>(this).FirstOrDefault() is not { } route)
            throw new InvalidOperationException("Route attribute not found");

        var basePath = route.Path;

        if (ReflectionUtils.GetMethodsFromAttribute<HttpMethodAttribute>(this, true) is not { } methods ||
            !methods.Any())
            throw new InvalidOperationException("No methods found");

        _routes = methods.Select(m => new RouteData(
            m.Key.Path is null ? basePath : $"{basePath}/{m.Key.Path}",
            m.Key.Path,
            m.Key.Method,
            m.Value
        )).ToList();
    }

    public async Task HandleRequestAsync(HttpListenerContext context, HttpListenerRequest request,
        ServiceProvider provider, string path)
    {
        Context = context;
        Request = request;
        Provider = provider;

        var currentRoute = _routes.FirstOrDefault(r => r.FullPath == path);
        if (currentRoute is null)
        {
            await Context.RespondAsync("Not found", HttpStatusCode.NotFound);
            return;
        }

        CurrentRoute = currentRoute;

        if (!await ExecuteChecksAsync()) return;

        var handle = await ExceptionUtils.TryAsync(() => (Task)CurrentRoute.MethodInfo.Invoke(this, null)!);
        if (handle.IsFailure)
        {
            Log.Error(handle.Exception, "Failed to execute handle");
            await Context.RespondAsync("Failed to execute handle", HttpStatusCode.InternalServerError);
        }
    }

    private async Task<bool> ExecuteChecksAsync()
    {
        if (CurrentRoute?.Method == Request.HttpMethod) return true;

        await Context.RespondAsync("Method not allowed", HttpStatusCode.MethodNotAllowed);
        return false;
    }

    protected record RouteData(string FullPath, string? OriginalPath, string Method, MethodInfo MethodInfo);
}