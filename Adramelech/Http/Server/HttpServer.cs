using System.Net;
using Adramelech.Http.Utilities;
using Adramelech.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Adramelech.Http.Server;

public class HttpServer
{
    private readonly List<ControllerBase> _controllers = [];
    private readonly List<MiddlewareBase> _middlewares = [];
    private readonly ServiceCollection _dependencies = [];

    public void AddControllers()
    {
        var endpoints = ErrorUtils.Try(() => ReflectionUtils.GetInstances<ControllerBase>());
        if (endpoints.IsFailure)
        {
            Log.Error(endpoints.Exception, "Failed to get controllers");
            return;
        }

        _controllers.AddRange(endpoints.Value!);
    }

    public void AddController(ControllerBase controller) => _controllers.Add(controller);

    public void AddMiddlewares()
    {
        var middlewares = ErrorUtils.Try(() => ReflectionUtils.GetInstances<MiddlewareBase>());
        if (middlewares.IsFailure)
        {
            Log.Error(middlewares.Exception, "Failed to get middlewares");
            return;
        }

        _middlewares.AddRange(middlewares.Value!);
    }

    public void AddMiddleware(MiddlewareBase middleware) => _middlewares.Add(middleware);

    public void AddDependency<T>(T dependency) =>
        _dependencies.AddSingleton(typeof(T), dependency!);

    public void Serve(int port, int accepts = 4)
    {
        accepts *= Environment.ProcessorCount;

        var listener = new HttpListener();
        listener.Prefixes.Add($"http://+:{port}/");

        var result = ErrorUtils.Try(listener.Start);
        if (result.IsFailure)
        {
            // If you get an error here, run the following command as administrator:
            // "netsh http add urlacl url=http://+:{0}/ user={1}"
            // Replace {0} with port and {1} with the equivalent of "everyone" in your language
            // For example, in English it's "everyone", in Portuguese and Spanish it's "todos"
            Log.Error(result.Exception, "Failed to start listener");
            return;
        }

        Log.Information("Listening on port {Port}", port);

        var sem = new Semaphore(accepts, accepts);

        while (listener.IsListening)
        {
            sem.WaitOne();

            listener.GetContextAsync().ContinueWith(async task =>
            {
                sem.Release();

                var context = await task;
                await ListenerCallbackAsync(context);
            });
        }

        listener.Stop();
    }

    private async Task ListenerCallbackAsync(HttpListenerContext context)
    {
        var request = context.Request;

        // Controllers
        var controller = _controllers.FirstOrDefault(c => c.Paths.Contains(request.Url!.AbsolutePath));
        if (controller is null)
        {
            await context.RespondAsync("Not found", HttpStatusCode.NotFound);
            return;
        }

        // Dependencies
        var provider = _dependencies.BuildServiceProvider();

        // Middlewares
        foreach (var middleware in _middlewares)
        {
            if (middleware.Path is not null)
                if (!middleware.Path.IsMatch(request.Url!.AbsolutePath))
                    continue;

            var result = await middleware.HandleRequestAsync(context, request, controller, provider);
            if (!result)
                return;
        }

        var handle = await ErrorUtils.TryAsync(() =>
            controller.HandleRequestAsync(context, request, provider, request.Url!.AbsolutePath));
        if (handle.IsFailure)
        {
            Log.Error(handle.Exception, "Failed to handle request");
            await context.RespondAsync("Internal server error", HttpStatusCode.InternalServerError);
        }
    }
}