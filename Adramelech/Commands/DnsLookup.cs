using System.Text;
using Adramelech.Configuration;
using Adramelech.Extensions;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class DnsLookup : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("dns-lookup", "Performs a DNS lookup on the given domain")]
    public async Task DnsLookupAsync([Summary("domain", "Domain to lookup")] string domain)
    {
        await DeferAsync();

        var response = await $"https://da.gd/dns/{domain}".Request<string>();
        if (response is null || response.Replace("\n", "").IsNullOrEmpty())
        {
            await Context.ErrorResponse("Invalid domain", true);
            return;
        }

        await FollowupWithFileAsync(
            embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithTitle("__DNS Lookup__")
                .AddField(":link: **Domain**", $"```{domain}```")
                .Build(),
            fileName: "domain.zone",
            fileStream: new MemoryStream(Encoding.UTF8.GetBytes(response!)));
    }
}