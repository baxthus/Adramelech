using System.Net;
using Adramelech.Http.Attributes;
using Adramelech.Http.Attributes.Http;
using Adramelech.Http.Attributes.Middleware;
using Adramelech.Http.Server;
using Adramelech.Http.Utilities;
using Adramelech.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Adramelech.Http.Controllers;

[ApiController]
[Route("auth")]
[NeedsToken]
public class AuthController : ControllerBase
{
    private ConfigService _configService = null!;

    protected override Task InitializeAsync()
    {
        _configService = Provider.GetRequiredService<ConfigService>();
        return Task.CompletedTask;
    }

    [HttpGet("setup-cookie")]
    public async Task SetupCookieAsync()
    {
        var cookie = new Cookie("token", _configService.ApiToken, "/")
        {
            Expires = DateTime.Now.AddDays(7),
            HttpOnly = true, // Prevents JavaScript from accessing the cookie
        };

        Context.Response.AppendCookie(cookie);

        await Context.RespondAsync("Cookie set successfully!\n" +
                                   "You can now use the API without having to provide a token.\n" +
                                   "But make sure to use the API at least one every 7 days, otherwise the cookie will expire and you'll have to set it again.");
    }
}