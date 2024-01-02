using System.Net;
using Adramelech.Common;
using Adramelech.Configuration;
using Adramelech.Http.Attributes;
using Adramelech.Http.Extensions;
using Adramelech.Http.Server;
using Adramelech.Utilities;
using Serilog;

namespace Adramelech.Http.Middlewares;

public class TokenAuthMiddleware : MiddlewareBase
{
    protected override async Task<bool> HandleRequestAsync()
    {
        var attribute = ExceptionUtils.Try(() =>
            ReflectionUtils.GetAttributes<NeedsTokenAttribute>(Endpoint).FirstOrDefault());
        if (attribute.IsFailure)
        {
            Log.Error(attribute.Exception, "Failed to get NeedsTokenAttribute");
            await Context.RespondAsync("Failed to verify if token is needed", HttpStatusCode.InternalServerError);
            return false;
        }

        var needsToken = attribute.Value is not null;

        if (!needsToken) return true;

        return await VerifyTokenAsync();
    }

    private async Task<bool> VerifyTokenAsync()
    {
        var result = GetTokens();
        if (result.IsFailure)
        {
            await Context.RespondAsync(result.Exception!.Message, HttpStatusCode.BadRequest);
            return false;
        }

        var rawTokens = result.Value!.ToAsyncEnumerable();

        var validToken = HttpConfig.Instance.ApiToken;
        if (string.IsNullOrEmpty(validToken))
        {
            await Context.RespondAsync("No token configured", HttpStatusCode.InternalServerError);
            return false;
        }

        var validTokenKey = HttpConfig.Instance.ApiTokenKey;
        if (string.IsNullOrEmpty(validTokenKey))
        {
            await Context.RespondAsync("No token key configured", HttpStatusCode.InternalServerError);
            return false;
        }

        var tokens = await rawTokens.SelectAwait(async token => new Token(
            token.IsEncrypted ? token.Value : await EncryptUtils.Encrypt(token.Value, validTokenKey)
        )).ToListAsync();

        if (tokens.Any(token => token.Value == validToken))
            return true;

        await Context.RespondAsync("Invalid token", HttpStatusCode.Forbidden);
        return false;
    }

    private Result<List<Token>> GetTokens()
    {
        var tokens = new List<Token>();

        // For browsers
        var cookie = Request.Cookies["token"];
        if (cookie is not null)
        {
            if (cookie.Expired)
            {
                Request.Cookies.Remove(cookie);
                return new Exception("Token cookie expired");
            }

            // Reset expiration date
            cookie.Expires = DateTime.Now.AddDays(7);
            // Token stored as a cookie is always encrypted
            tokens.Add(new Token(cookie.Value, true));
        }

        // For clients
        var header = Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(header))
        {
            if (!header.StartsWith("Bearer "))
                return new Exception("Invalid token header");
            tokens.Add(new Token(header.Replace("Bearer ", string.Empty)));
        }

        // For anyone
        var query = Request.QueryString["token"];
        if (!string.IsNullOrEmpty(query))
            tokens.Add(new Token(query));

        return tokens.Count == 0
            ? new Exception("Missing token header, cookie or query parameter")
            : tokens;
    }

    private record Token(string Value, bool IsEncrypted = false);
}