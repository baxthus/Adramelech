using System.Text;
using Adramelech.Extensions;
using Adramelech.Utilities;
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
        if (response.IsNullOrEmpty())
        {
            await Context.SendError("Invalid domain", true);
            return;
        }

        var records = ParseResponse(response!).ToList();

        var typeMax = records.Max(x => x.Type.Length) > "Type".Length
            ? records.Max(x => x.Type.Length)
            : "Type".Length;
        var contentMax = records.Max(x => x.Content.Length) > "Type".Length
            ? records.Max(x => x.Content.Length)
            : "Content".Length;
        var revalidateMax = "Revalidate In".Length; // "Revalidate In" is the longest possible value

        StringBuilder content = new();

        content.AppendLine("\u250C Type ".PadRight(typeMax + 2, '\u2500') +
                           "\u252C Revalidate In ".PadRight(revalidateMax + 2, '\u2500') +
                           "\u2510" + " Content ".Centralize(contentMax + 2, '\u2500') + "\u2510");

        foreach (var record in records)
        {
            content.AppendLine(
                $"\u2502 {record.Type.PadRight(typeMax)} " +
                $"\u2502 {record.RevalidateIn.PadRight(revalidateMax)} " +
                $"\u2502 {record.Content.PadRight(contentMax)} \u2502");

            // If this is the last record, draw a bottom border
            if (record.Equals(records.Last()))
            {
                content.AppendLine("\u2514".PadRight(typeMax + 3, '\u2500') +
                                   "\u2534".PadRight(revalidateMax + 3, '\u2500') +
                                   "\u2534".PadRight(contentMax + 3, '\u2500') + "\u2518");
                break;
            }

            content.AppendLine("\u251C".PadRight(typeMax + 3, '\u2500') +
                               "\u253C".PadRight(revalidateMax + 3, '\u2500') +
                               "\u253C".PadRight(contentMax + 3, '\u2500') + "\u2524");
        }

        await FollowupWithFileAsync(
            text: "> ## DNS Lookup\n\n" +
                  ":link: **Domain**\n" +
                  $"```{domain}```\n" +
                  $":page_with_curl: **Records**",
            fileName: "domain.zone",
            fileStream: new MemoryStream(Encoding.UTF8.GetBytes(content.ToString())));
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