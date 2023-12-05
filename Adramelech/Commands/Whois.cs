using System.Text;
using Adramelech.Configuration;
using Adramelech.Extensions;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Whois : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("whois", "Get information about a domain or IP address")]
    public async Task WhoisAsync([Summary("target", "The domain or IP address to get information about")] string target)
    {
        await DeferAsync();

        var response = await $"https://da.gd/w/{target}".Request<string>();
        if (response.IsNullOrEmpty() || BadStrings.Any(response!.Trim().Contains))
        {
            await Context.SendError("Error getting information about the target", true);
            return;
        }

        await FollowupWithFileAsync(
            embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithTitle("__Whois__")
                .AddField(":link: Target", $"```{target}```")
                .WithFooter("Powered by da.gd")
                .Build(),
            fileName: $"{target}.txt",
            fileStream: new MemoryStream(Encoding.UTF8.GetBytes(response)));
    }

    private static readonly string[] BadStrings =
    {
        "Malformed",
        "Wrong",
        "The queried object does not",
        "Invalid",
        "No match",
        "Domain not",
        "NOT FOUND"
    };
}