using System.Net;
using System.Text;
using Adramelech.Configuration;
using Adramelech.Http.Attributes;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Http.Common;

public abstract class EndpointBase
{
    private HttpListenerContext _context = null!;
    protected HttpListenerRequest Request = null!;
    protected DiscordSocketClient BotClient = null!;

    public readonly string? Path;
    private readonly string? _method;
    private bool _needsToken;

    protected EndpointBase()
    {
        if (GetAttribute<EndpointAttribute>(this) is not { } endpointAttribute)
            throw new InvalidOperationException("Endpoint attribute not found");

        Path = endpointAttribute.Path;
        _method = endpointAttribute.Method;
    }

    public async Task HandleRequestAsync(HttpListenerContext context, HttpListenerRequest request,
        DiscordSocketClient botClient)
    {
        _context = context;
        Request = request;
        BotClient = botClient;

        if (GetAttribute<NeedsTokenAttribute>(this) is not null)
            _needsToken = true;

        if (!await ExecuteCheckAsync()) return;

        try
        {
            await HandleAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to handle request");
            await RespondAsync("Internal server error", HttpStatusCode.InternalServerError);
        }
    }

    protected abstract Task HandleAsync();

    internal async Task RespondAsync(ReadOnlyMemory<byte> buffer, HttpStatusCode statusCode = HttpStatusCode.OK,
        string contentType = "text/plain")
    {
        var response = _context.Response;

        response.StatusCode = statusCode.GetHashCode();
        response.ContentType = contentType;
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);

        response.Close();
    }

    internal Task RespondAsync(string content, HttpStatusCode statusCode = HttpStatusCode.OK,
        string contentType = "text/plain") =>
        RespondAsync(Encoding.UTF8.GetBytes(content), statusCode, contentType);

    internal byte[]? GetBody()
    {
        using BinaryReader r = new(Request.InputStream);
        var buffer = r.ReadBytes(Convert.ToInt32(Request.ContentLength64));

        return buffer.Length > 0 ? buffer : null;
    }

    private static T? GetAttribute<T>(object obj) where T : Attribute =>
        obj.GetType().GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;

    private async Task<bool> ExecuteCheckAsync()
    {
        if (_method == Request.HttpMethod) return _needsToken is not true || await VerifyToken();

        await RespondAsync("Method Not Allowed", HttpStatusCode.MethodNotAllowed);
        return false;
    }

    private async Task<bool> VerifyToken()
    {
        var token = Request.QueryString["token"];
        if (string.IsNullOrEmpty(token))
        {
            await RespondAsync("Missing token parameter", HttpStatusCode.BadRequest);
            return false;
        }

        var validToken = HttpConfig.Instance.ApiToken;
        if (string.IsNullOrEmpty(validToken))
        {
            await RespondAsync("No token configured", HttpStatusCode.InternalServerError);
            return false;
        }

        if (token == HttpConfig.Instance.ApiToken) return true;

        await RespondAsync("Invalid token", HttpStatusCode.Unauthorized);
        return false;
    }
}