using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Flurl;

namespace Adramelech.Commands;

public class Short : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("short", "Shorten a URL")]
    public async Task ShortAsync([Summary("url", "The URL to shorten")] string url)
    {
        var response = await Utilities.Request<string>(new Url("https://is.gd")
            .AppendPathSegment("create.php")
            .SetQueryParam("format", "simple")
            .SetQueryParam("url", url));
        if (response.IsInvalid() || response!.Replace("\n", "").StartsWith("Error"))
        {
            await Context.ErrorResponse("Error shortening URL");
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Shortened URL__")
            .AddField(":outbox_tray: Original URL", url)
            .AddField(":inbox_tray: Shortened URL", response)
            .WithFooter("Powered by is.gd")
            .Build();

        await RespondAsync(embed: embed);
    }
}