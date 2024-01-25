using System.Net;
using Adramelech.Common;
using Adramelech.Http.Attributes.Middleware;
using Adramelech.Http.Server;
using Adramelech.Http.Utilities;
using Adramelech.Services;
using Adramelech.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Adramelech.Http.Middlewares;

public class TokenAuthMiddleware : MiddlewareBase
{
    private ConfigService _configService = null!;

    protected override async Task<bool> HandleAsync()
    {
        _configService = Provider.GetRequiredService<ConfigService>();

        var attribute = ErrorUtils.Try(() =>
            ReflectionUtils.GetAttributes<NeedsTokenAttribute>(Controller).FirstOrDefault());
        if (attribute.IsFailure)
        {
            Log.Error(attribute.Exception, "Failed to get NeedsTokenAttribute");
            await Context.RespondAsync("Failed to verify if token is needed", HttpStatusCode.InternalServerError);
            return false;
        }

        var needsToken = attribute.Value is not null;

        if (!needsToken)
            return true;

        return await VerifyTokenAsync(attribute.Value!.Priority);
    }

    private async Task<bool> VerifyTokenAsync(TokenSources priority)
    {
        var result = GetTokens();
        if (result.IsFailure)
        {
            await Context.RespondAsync(result.Exception!.Message, HttpStatusCode.Unauthorized);
            return false;
        }

        var rawTokens = result.Value!.ToAsyncEnumerable();

        if (string.IsNullOrEmpty(_configService.ApiTokenKey))
        {
            await Context.RespondAsync("No token configured", HttpStatusCode.InternalServerError);
            return false;
        }

        if (string.IsNullOrEmpty(_configService.ApiTokenKey))
        {
            await Context.RespondAsync("No token key configured", HttpStatusCode.InternalServerError);
            return false;
        }

        // Encrypt token, except for the cookie
        var tokens = await rawTokens.SelectAwait(async token =>
            token with
            {
                Value = token.Source == TokenSources.Cookie
                    ? token.Value
                    : await EncryptUtils.Encrypt(token.Value, _configService.ApiTokenKey)
            }).ToListAsync();

        // Test the priority token first
        var priorityToken = tokens.FirstOrDefault(token => token.Source == priority);
        if (priorityToken is not null)
            if (priorityToken.Value == _configService.ApiToken)
                return true;

        // Test the other tokens
        var otherTokens = tokens.Where(token => token.Source != priority);
        if (otherTokens.Any(token => token.Value == _configService.ApiToken))
            return true;

        await Context.RespondAsync("Invalid token", HttpStatusCode.Unauthorized);
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
                return new Exception("Token cookie has expired");
            }

            // Reset expiration date
            cookie.Expires = DateTime.Now.AddDays(7);
            cookie.Path = "/";
            Context.Response.AppendCookie(cookie);

            tokens.Add(new Token(cookie.Value, TokenSources.Cookie));
        }

        // For clients
        var header = Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(header))
        {
            if (!header.StartsWith("Bearer "))
                return new Exception("Invalid authorization header");
            tokens.Add(new Token(header.Replace("Bearer ", string.Empty), TokenSources.Header));
        }

        // For anyone
        var query = Request.QueryString["token"];
        if (!string.IsNullOrEmpty(query))
            tokens.Add(new Token(query, TokenSources.QueryString));

        return tokens.Count > 0 ? tokens : new Exception("Missing token header, cookie or query parameter");
    }

    private record Token(string Value, TokenSources Source);
}