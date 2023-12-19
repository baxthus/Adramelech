using System.Net;
using Adramelech.Configuration;
using Adramelech.Http.Attributes;
using Adramelech.Http.Extensions;
using Adramelech.Utilities;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Http.Common;

public abstract class EndpointBase
{
    protected HttpListenerContext Context = null!;
    protected HttpListenerRequest Request = null!;
    protected DiscordSocketClient BotClient = null!;

    public readonly string? Path;
    private readonly string? _method;
    private bool _needsToken;

    protected EndpointBase()
    {
        if (GetAttribute<EndpointAttribute>(this) is not { } endpoint)
            throw new InvalidOperationException("Endpoint attribute not found");

        Path = endpoint.Path;
        _method = endpoint.Method;
    }

    public async Task HandleRequestAsync(HttpListenerContext context, HttpListenerRequest request,
        DiscordSocketClient botClient)
    {
        Context = context;
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
            await context.RespondAsync("Internal server error", HttpStatusCode.InternalServerError);
        }
    }

    protected abstract Task HandleAsync();

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

        await Context.RespondAsync("Method Not Allowed", HttpStatusCode.MethodNotAllowed);
        return false;
    }

    private async Task<bool> VerifyToken()
    {
        var token = Request.QueryString["token"];
        if (string.IsNullOrEmpty(token))
        {
            await Context.RespondAsync("Missing token parameter", HttpStatusCode.BadRequest);
            return false;
        }

        var validToken = HttpConfig.Instance.ApiToken;
        if (string.IsNullOrEmpty(validToken))
        {
            await Context.RespondAsync("No token configured", HttpStatusCode.InternalServerError);
            return false;
        }

        var validTokenSalt = HttpConfig.Instance.ApiTokenSalt;
        if (string.IsNullOrEmpty(validTokenSalt))
        {
            await Context.RespondAsync("No token salt configured", HttpStatusCode.InternalServerError);
            return false;
        }

        if (EncryptUtils.CompareHash(token, validToken, validTokenSalt)) return true;

        await Context.RespondAsync("Invalid token", HttpStatusCode.Forbidden);
        return false;
    }
}