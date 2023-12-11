using System.Net;
using System.Reflection;
using System.Text;
using Adramelech.Http.Common;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Http;

public class HttpServer
{
    private readonly HttpListener _listener;
    private readonly DiscordSocketClient _botClient;
    private volatile bool _stop = true;

    public HttpServer(DiscordSocketClient botClient, int? port = null)
    {
        _botClient = botClient;

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port ?? 8000}/");
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
            await Respond(context, "Invalid endpoint", statusCode: 404);
            return;
        }

        await endpoint.HandleRequestAsync(context, request, _botClient);
    }

    private static IEnumerable<EndpointBase> GetEndpoints() =>
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(EndpointBase)))
            .Select(t => (EndpointBase)Activator.CreateInstance(t)!);

    private static async Task Respond(HttpListenerContext context, string content, string contentType = "text/plain",
        int statusCode = 200)
    {
        var response = context.Response;
        var buffer = Encoding.UTF8.GetBytes(content);

        response.StatusCode = statusCode;
        response.ContentType = contentType;
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);

        response.Close();
    }

    private void Stop() => _listener.Stop();
}