using System.Net;
using System.Reflection;
using Adramelech.Http.Common;
using Adramelech.Http.Extensions;
using Adramelech.Utilities;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Http;

/// <summary>
/// Represents a HTTP server
/// </summary>
public class HttpServer
{
    private readonly HttpListener _listener;
    private readonly DiscordSocketClient _botClient;
    private volatile bool _stop = true;

    /// <summary>
    /// Creates a new instance of <see cref="HttpServer"/>
    /// </summary>
    /// <param name="botClient">The <see cref="DiscordSocketClient"/> to use</param>
    /// <param name="port">The port to listen on; defaults to 8000</param>
    public HttpServer(DiscordSocketClient botClient, int port = 8000)
    {
        _botClient = botClient;

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://+:{port}/");
    }

    /// <summary>
    /// Starts the server
    /// </summary>
    public async Task InitializeAsync()
    {
        var ex = ExceptionUtils.Try(_listener.Start);
        if (ex is not null)
        {
            Log.Error(ex, "Failed to start HttpListener");
            return;
        }

        _stop = false;

        Log.Information("Server listening at {Prefixes}", _listener.Prefixes);

        while (_listener.IsListening)
        {
            var context = await _listener.GetContextAsync();

            await ListenerCallbackAsync(context);

            if (_stop) Stop();
        }

        Stop();
    }

    /// <summary>
    /// The callback for the listener
    /// </summary>
    /// <param name="context">The <see cref="HttpListenerContext"/> to use</param>
    private async Task ListenerCallbackAsync(HttpListenerContext context)
    {
        var request = context.Request;

        var (endpoint, ex) =
            ExceptionUtils.Try(() => GetEndpoints().FirstOrDefault(e => e.Path == request.Url?.AbsolutePath));
        if (ex is not null)
        {
            Log.Error(ex, "Failed to get endpoint");
            await context.RespondAsync("Internal server error", HttpStatusCode.InternalServerError);
            return;
        }

        if (endpoint is null)
        {
            await context.RespondAsync("Invalid endpoint", HttpStatusCode.NotFound);
            return;
        }

        var exception = await ExceptionUtils.TryAsync(() => endpoint.HandleRequestAsync(context, request, _botClient));
        if (exception is not null)
        {
            Log.Error(exception, "Failed to handle request");
            await context.RespondAsync("Internal server error", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Gets all endpoints
    /// </summary>
    /// <returns>The endpoints</returns>
    /// <remarks>Can throw everything</remarks>
    private static IEnumerable<EndpointBase> GetEndpoints() =>
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(EndpointBase)))
            .Select(t => (EndpointBase)Activator.CreateInstance(t)!);

    /// <summary>
    /// Stops the server
    /// </summary>
    private void Stop() => _listener.Stop();
}