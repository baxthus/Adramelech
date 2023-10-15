using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Whois : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("whois", "Get information about a domain or IP address")]
    public async Task WhoisAsync([Summary("target", "The domain or IP address to get information about")] string target)
    {
        var response = await Utilities.Request<string>($"https://da.gd/w/{target}");
        if (response.IsInvalid() || BadStrings.Any(response!.Contains))
        {
            await Context.ErrorResponse("Error getting information about the target");
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Whois__")
            .AddField(":link: Target", $"```{target}```")
            .WithFooter("Powered by da.gd")
            .Build();
        
        using var file = new MemoryStream(Encoding.UTF8.GetBytes(response!));

        await RespondWithFileAsync(embed: embed, fileName: $"{target}.txt", fileStream: file);
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