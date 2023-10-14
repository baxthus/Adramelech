using System.Text;
using Discord;
using Discord.Interactions;

namespace Adramelech.Commands;

public class DnsLookup : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("dns-lookup", "Performs a DNS lookup on the given domain")]
    public async Task DnsLookupAsync([Summary("domain", "Domain to lookup")] string domain)
    {
        var response = await Utilities.Request<string>($"https://da.gd/dns/{domain}");
        if (response.IsInvalid())
        {
            await Context.ErrorResponse("Invalid domain");
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__DNS Lookup__")
            .AddField(":link: **Domain**", $"```{domain}```")
            .Build();
        
        var file = new MemoryStream(Encoding.UTF8.GetBytes(response!));

        await RespondWithFileAsync(embed: embed, fileName: "domain.zone", fileStream: file);
    }
}