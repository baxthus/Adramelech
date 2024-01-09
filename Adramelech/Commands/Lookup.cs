using System.Text.RegularExpressions;
using Adramelech.Extensions;
using Adramelech.Services;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Adramelech.Commands;

public class Lookup : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("lookup", "Look up a ip or domain")]
    public async Task LookupAsync([Summary("local", "Address to lookup (ip or domain)")] string local)
    {
        await DeferAsync();

        var ip = LookupHelper.IpRegex().Match(local).Success
            ? local
            : await GetIpFromDomain(local);
        if (ip.IsNullOrEmpty())
        {
            await Context.SendError("Failed to lookup ip", true);
            return;
        }

        var response = await $"https://ipwho.is/{ip}".Request<Whois>("curl");
        if (response.IsDefault() || !response.Success)
        {
            await Context.SendError("Error while looking up ip", true);
            return;
        }

        var domain = local != ip ? local : "None";

        var mainField = $"**IP:** {ip}\n" +
                        $"**Domain:** {domain}\n" +
                        $"**Type:** {response.Type}";

        var locationField = $"**Continent:** {response.Continent}\n" +
                            $"**Country:** {response.Country} :flag_{response.CountryCode.ToLower()}:\n" +
                            $"**Region:** {response.Region}\n" +
                            $"**City:** {response.City}\n" +
                            $"**Latitude:** {response.Latitude}\n" +
                            $"**Longitude:** {response.Longitude}\n" +
                            $"**Postal:** {response.Postal}";

        var connectionField = $"**ASN:** {response.Connection.Asn}\n" +
                              $"**Org:** {response.Connection.Org}\n" +
                              $"**ISP:** {response.Connection.Isp}\n" +
                              $"**Domain:** {response.Connection.Domain}";

        var timezoneField = $"**ID:** {response.Timezone.Id}\n" +
                            $"**UTC:** {response.Timezone.Utc}\n" +
                            $"**Offset:** {response.Timezone.Offset}";

        var mapsUrl = $"https://www.google.com/maps/search/?api=1&query={response.Latitude},{response.Longitude}";

        await FollowupAsync(
            embed: new EmbedBuilder()
                .WithColor(ConfigService.EmbedColor)
                .WithDescription("For the best results, search by IP")
                .AddField(":zap: **Main**", mainField)
                .AddField(":earth_americas: **Location**", locationField)
                .AddField(":satellite: **Connection**", connectionField)
                .AddField(":clock1: **Timezone**", timezoneField)
                .WithFooter("Powered by ipwhois.io")
                .Build(),
            components: new ComponentBuilder()
                // Emoji is :earth_americas:
                .WithButton("Maps", url: mapsUrl, style: ButtonStyle.Link, emote: new Emoji("\uD83C\uDF0E"))
                .Build());
    }

    private static async Task<string?> GetIpFromDomain(string domain)
    {
        var response = await $"https://da.gd/host/{domain}".Request<string>();
        if (response.IsNullOrEmpty())
            return null;

        response = response!.Trim();

        if (!response.StartsWith("No"))
            return response.Contains(',') ? response[..response.IndexOf(',')] : response;

        return null;
    }

    private struct Whois
    {
        public bool Success { get; set; }
        public string Type { get; set; }
        public string Continent { get; set; }
        public string Country { get; set; }
        [JsonProperty("country_code")] public string CountryCode { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Postal { get; set; }
        public InternalConnection Connection { get; init; }
        public InternalTimezone Timezone { get; init; }

        internal struct InternalConnection
        {
            public int Asn { get; set; }
            public string Org { get; set; }
            public string Isp { get; set; }
            public string Domain { get; set; }
        }

        internal struct InternalTimezone
        {
            public string Id { get; set; }
            public int Offset { get; set; }
            public string Utc { get; set; }
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
internal partial class LookupHelper
{
    [GeneratedRegex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b")]
    public static partial Regex IpRegex();
}