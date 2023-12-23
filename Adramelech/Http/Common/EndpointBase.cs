using System.Net;
using System.Reflection;
using Adramelech.Configuration;
using Adramelech.Http.Attributes;
using Adramelech.Http.Extensions;
using Adramelech.Utilities;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Http.Common;

/// <summary>
/// The base class for all endpoints.
/// </summary>
public abstract class EndpointBase
{
    protected HttpListenerContext Context = null!;
    protected HttpListenerRequest Request = null!;
    protected DiscordSocketClient BotClient = null!;

    public readonly string? Path;
    private readonly string? _method;
    private bool _needsToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointBase" /> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Endpoint attribute not found</exception>
    /// <remarks>Throws the same as <see cref="GetAttribute{T}"/>.</remarks>
    protected EndpointBase()
    {
        if (GetAttribute<EndpointAttribute>(this) is not { } endpoint)
            throw new InvalidOperationException("Endpoint attribute not found");

        Path = endpoint.Path;
        _method = endpoint.Method;
    }

    /// <summary>
    /// Handles the request asynchronously.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="request">The request.</param>
    /// <param name="botClient">The discord socket bot client.</param>
    public async Task HandleRequestAsync(HttpListenerContext context, HttpListenerRequest request,
        DiscordSocketClient botClient)
    {
        Context = context;
        Request = request;
        BotClient = botClient;

        var (needsToken, exception) = ExceptionUtils.Try(() => GetAttribute<NeedsTokenAttribute>(this));
        if (exception is not null)
        {
            Log.Error(exception, "Failed to get NeedsTokenAttribute");
            await context.RespondAsync("Failed to verify if token is needed", HttpStatusCode.InternalServerError);
            return;
        }

        _needsToken = needsToken is not null;

        var (valid, e) = await ExceptionUtils.TryAsync(ExecuteCheckAsync);
        if (e is not null)
        {
            Log.Error(e, "Failed to execute check");
            await context.RespondAsync("Failed to execute check", HttpStatusCode.InternalServerError);
            return;
        }

        if (!valid) return;

        var ex = await ExceptionUtils.TryAsync(HandleAsync);
        if (ex is not null)
        {
            Log.Error(ex, "Failed to handle request");
            await context.RespondAsync("Internal server error", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// The method to handle the request.
    /// </summary>
    /// <returns>A task.</returns>
    /// <remarks>Can throw anything.</remarks>
    protected abstract Task HandleAsync();

    /// <summary>
    /// Gets the body of the request.
    /// </summary>
    /// <returns>The body.</returns>
    /// <exception cref="InvalidOperationException">Body is empty</exception>
    /// <remarks>Throws the same as <see cref="BinaryReader.ReadBytes(int)"/> and <see cref="Convert.ToInt32(long)"/>.</remarks>
    internal byte[] GetBody()
    {
        using BinaryReader r = new(Request.InputStream);
        var buffer = r.ReadBytes(Convert.ToInt32(Request.ContentLength64));

        return buffer.Length > 0 ? buffer : throw new InvalidOperationException("Body is empty");
    }

    /// <summary>
    /// Gets the attribute of the specified type.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <typeparam name="T">The attribute type.</typeparam>
    /// <returns>The attribute or null.</returns>
    /// <remarks>Throws the same as <see cref="MemberInfo.GetCustomAttributes(Type, bool)"/> and <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/>.</remarks>
    private static T? GetAttribute<T>(object obj) where T : Attribute =>
        obj.GetType().GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;

    /// <summary>
    /// Executes the check's for the request.
    /// </summary>
    /// <returns>True if the request is valid, false otherwise.</returns>
    /// <remarks>Throws the same as <see cref="VerifyToken"/>.</remarks>
    private async Task<bool> ExecuteCheckAsync()
    {
        if (_method == Request.HttpMethod) return _needsToken is not true || await VerifyToken();

        await Context.RespondAsync("Method Not Allowed", HttpStatusCode.MethodNotAllowed);
        return false;
    }

    /// <summary>
    /// Verifies the token.
    /// </summary>
    /// <returns>True if the token is valid, false otherwise.</returns>
    /// <remarks>Throws the same as <see cref="EncryptUtils.CompareHash(string, string, string)"/>.</remarks>
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