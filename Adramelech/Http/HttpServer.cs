using System.Net;
using System.Reflection;
using Adramelech.Http.Common;
using Adramelech.Http.Extensions;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Http;

public class HttpServer
{
    private readonly HttpListener _listener;
    private readonly DiscordSocketClient _botClient;
    private volatile bool _stop = true;

    public HttpServer(DiscordSocketClient botClient, int port = 8000)
    {
        _botClient = botClient;

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://+:{port}/");
    }

    public async Task InitializeAsync()
    {
        try
        {
            _listener.Start();
            _stop = false;
        }
        catch (HttpListenerException e)
        {
            Log.Error(e, "Failed to start HttpListener");
            return;
        }

        Log.Information("Server listening at {Prefixes}", _listener.Prefixes);

        while (_listener.IsListening)
        {
            var context = await _listener.GetContextAsync();

            await ListenerCallbackAsync(context);

            if (_stop) Stop();
        }

        Stop();
    }

    private async Task ListenerCallbackAsync(HttpListenerContext context)
    {
        var request = context.Request;

        var endpoint = GetEndpoints().FirstOrDefault(e => e.Path == request.Url?.AbsolutePath);
        if (endpoint is null)
        {
            await context.RespondAsync("Invalid endpoint", HttpStatusCode.NotFound);
            return;
        }

        try
        {
            await endpoint.HandleRequestAsync(context, request, _botClient);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to handle request");
            await context.RespondAsync("Internal server error", HttpStatusCode.InternalServerError);
        }
    }

    private static IEnumerable<EndpointBase> GetEndpoints() =>
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(EndpointBase)))
            .Select(t => (EndpointBase)Activator.CreateInstance(t)!);

    private void Stop() => _listener.Stop();
}