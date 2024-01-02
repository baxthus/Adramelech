using System.Net;
using Adramelech.Configuration;
using Adramelech.Http.Attributes;
using Adramelech.Http.Extensions;
using Adramelech.Http.Server;

namespace Adramelech.Http.Endpoints.Auth;

[Endpoint("/auth/setup-cookie")]
[NeedsToken]
public class SetupCookie : EndpointBase
{
    protected override async Task HandleAsync()
    {
        var token = HttpConfig.Instance.ApiToken;

        var cookie = new Cookie("token", token, "/")
        {
            Expires = DateTime.Now.AddDays(7),
            HttpOnly = true, // Prevents JavaScript from accessing the cookie
        };

        Context.Response.AppendCookie(cookie);

        await Context.RespondAsync("Cookie set successfully!\n" +
                                   "You can now use the API without having to provide a token.\n" +
                                   "But be sure to use the API at least once every 7 days, otherwise the cookie will expire and you'll have to set it again.");
    }
}