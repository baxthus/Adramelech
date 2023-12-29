using System.Text;
using Adramelech.Extensions;
using Adramelech.Tools;
using Adramelech.Utilities;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class DnsLookup : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("dns-lookup", "Performs a DNS lookup on the given domain")]
    public async Task DnsLookupAsync([Summary("domain", "Domain to lookup")] string domain,
        [Summary("separate-rows", "Whether to separate rows with lines")]
        bool separateRows = false)
    {
        await DeferAsync();

        var response = await $"https://da.gd/dns/{domain}".Request<string>();
        if (response.IsNullOrEmpty())
        {
            await Context.SendError("Invalid domain", true);
            return;
        }

        var records = ParseResponse(response!).ToList();

        var content = new UnicodeSheet(separateRows)
            .AddColumn("Type", records.Select(x => x.Type))
            .AddColumn("Revalidate In", records.Select(x => x.RevalidateIn))
            .AddColumn("Content", records.Select(x => x.Content))
            .Build();

        await FollowupWithFileAsync(
            text: "> ## DNS Lookup\n\n" +
                  "### :link: **Domain**\n" +
                  $"```{domain}```\n" +
                  "### :page_with_curl: **Records**",
            fileName: "domain.zone",
            fileStream: new MemoryStream(Encoding.UTF8.GetBytes(content)));
    }

    // Format: <DOMAIN> <REVALIDATE-IN> IN <TYPE> <CONTENT>
    private static IEnumerable<DnsRecord> ParseResponse(string text) =>
        from line in text.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x))
        let parts = line.Split(' ')
        where parts.Length >= 5
        select new DnsRecord
        {
            Type = parts[3],
            RevalidateIn = parts[1],
            Content = string.Join(" ", parts.Skip(4)).TrimEnd(' ')
        };

    private struct DnsRecord
    {
        public string Type { get; init; }
        public string RevalidateIn { get; init; }
        public string Content { get; init; }
    }
}