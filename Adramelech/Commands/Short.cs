using Adramelech.Extensions;
using Adramelech.Services;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Short : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("short", "Shorten a URL")]
    public async Task ShortAsync([Summary("url", "The URL to shorten")] string url)
    {
        await DeferAsync();

        var response = await $"https://is.gd/create.php?format=simple&url={url}".Request<string>();
        if (response.IsNullOrEmpty() || response!.Trim().StartsWith("Error"))
        {
            await Context.SendError("Error shortening URL", true);
            return;
        }

        await FollowupAsync(embed: new EmbedBuilder()
            .WithColor(ConfigService.EmbedColor)
            .WithTitle("__Shortened URL__")
            .AddField(":outbox_tray: Original URL", url)
            .AddField(":inbox_tray: Shortened URL", response)
            .WithFooter("Powered by is.gd")
            .Build());
    }
}