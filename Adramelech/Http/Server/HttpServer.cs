using System.Net;
using Adramelech.Http.Extensions;
using Adramelech.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Adramelech.Http.Server;

public class HttpServer
{
    private readonly List<EndpointBase> _endpoints = [];
    private readonly List<MiddlewareBase> _middlewares = [];
    private readonly List<KeyValuePair<Type, object>> _dependencies = [];

    /// <summary>
    /// Adds all endpoints that inherit from <see cref="EndpointBase" /> to the server
    /// </summary>
    public void AddEndpoints()
    {
        var endpoints = ExceptionUtils.Try(ReflectionUtils.GetInstances<EndpointBase>);
        if (endpoints.IsFailure)
        {
            Log.Error(endpoints.Exception, "Failed to get endpoints");
            return;
        }

        _endpoints.AddRange(endpoints.Value!);
    }

    /// <summary>
    /// Adds an endpoint to the server
    /// </summary>
    /// <param name="endpoint">The endpoint to add</param>
    public void AddEndpoint(EndpointBase endpoint) => _endpoints.Add(endpoint);

    /// <summary>
    /// Adds all middlewares that inherit from <see cref="MiddlewareBase" /> to the server
    /// </summary>
    public void AddMiddlewares()
    {
        var middlewares = ExceptionUtils.Try(ReflectionUtils.GetInstances<MiddlewareBase>);
        if (middlewares.IsFailure)
        {
            Log.Error(middlewares.Exception, "Failed to get middlewares");
            return;
        }

        _middlewares.AddRange(middlewares.Value!);
    }

    /// <summary>
    /// Adds a middleware to the server
    /// </summary>
    /// <param name="middleware">The middleware to add</param>
    public void AddMiddleware(MiddlewareBase middleware) => _middlewares.Add(middleware);

    /// <summary>
    /// Adds a dependency to the server
    /// </summary>
    /// <param name="dependency">The dependency to add</param>
    /// <typeparam name="T">The type of the dependency</typeparam>
    public void AddDependency<T>(T dependency) =>
        _dependencies.Add(new KeyValuePair<Type, object>(typeof(T), dependency!));

    /// <summary>
    /// Starts the server
    /// </summary>
    /// <param name="port">The port to listen on</param>
    public async Task ServeAsync(int port)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://+:{port}/");

        var result = ExceptionUtils.Try(listener.Start);
        if (result.IsFailure)
        {
            Log.Error(result.Exception, "Failed to start HttpListener");
            return;
        }

        Log.Information("Server listening at {Prefixes}", listener.Prefixes);

        while (listener.IsListening)
        {
            var context = await listener.GetContextAsync();
            await ListenerCallbackAsync(context);
        }

        listener.Stop();
    }

    private async Task ListenerCallbackAsync(HttpListenerContext context)
    {
        var request = context.Request;

        // Endpoints
        var endpoint = _endpoints.FirstOrDefault(e => e.Path == request.Url!.AbsolutePath);
        if (endpoint is null)
        {
            await context.RespondAsync("Not found", HttpStatusCode.NotFound);
            return;
        }

        // Middlewares
        foreach (var middleware in _middlewares)
        {
            var result = await middleware.HandleRequestAsync(context, request, endpoint);
            // If the middleware returns false, the response has already been sent
            if (!result)
                return;
        }

        // Dependencies
        var services = new ServiceCollection();
        _dependencies.ForEach(obj => services.AddSingleton(obj.Key, obj.Value));
        var provider = services.BuildServiceProvider();

        var handle = await ExceptionUtils.TryAsync(() => endpoint.HandleRequestAsync(context, request, provider));
        if (handle.IsFailure)
        {
            Log.Error(handle.Exception, "Failed to handle request");
            await context.RespondAsync("Internal server error", HttpStatusCode.InternalServerError);
        }
    }
}