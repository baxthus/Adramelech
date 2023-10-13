﻿using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Flurl;
using Newtonsoft.Json;

namespace Adramelech.Commands;

public class LookupCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("lookup", "Look up a ip or domain")]
    public async Task Lookup([Summary("local", "Address to lookup (ip or domain)")] string local)
    {
        var ip = LookupCommandExtensions.IpRegex().Match(local).Success
            ? local
            : await GetIpFromDomain(local);
        if (ip.IsInvalid())
        {
            await Context.ErrorResponse("Invalid ip");
            return;
        }

        var response = await Utilities.Request<Whois>($"https://ipwho.is/{ip}", "curl");
        if (response.IsInvalid() || !response.Success)
        {
            await Context.ErrorResponse("Error while looking up ip");
            return;
        }

        var domain = local != ip ? local : "None";
        var mainField = $"**IP:** {ip}\n" +
                        $"**Domain:** {domain}\n" +
                        $"**Type:** {response.Type}";

        var locationField = $"**Continent:** {response.Continent}\n" +
                            $"**Country:** {response.Country} :flag_{response.CountryCode.ToLower()}\n" +
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

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithDescription("For the best results, search by IP")
            .AddField(":zip: **Main**", mainField)
            .AddField(":earth_americas: **Location**", locationField)
            .AddField(":satellite: **Connection**", connectionField)
            .AddField(":clock1: **Timezone**", timezoneField)
            .WithFooter("Powered by ipwhois.io")
            .Build();

        var mapsUrl = new Url("https://www.google.com")
            .AppendPathSegments("maps", "search", "/")
            .SetQueryParam("api", 1)
            .SetQueryParam("query", $"{response.Latitude},{response.Longitude}");

        var button = new ComponentBuilder()
            // Emoji is :earth_americas:
            .WithButton("Maps", url: mapsUrl, style: ButtonStyle.Link, emote: new Emoji("\uD83C\uDF0E"))
            .Build();

        await RespondAsync(embed: embed, components: button);
    }

    private async Task<string?> GetIpFromDomain(string domain)
    {
        var response = await Utilities.Request<string>($"https://da.gd/host/{domain}");
        if (response.IsInvalid())
        {
            await Context.ErrorResponse("Invalid domain");
            return null;
        }

        var ip = response!.Replace("\n", "");

        if (!ip.StartsWith("No"))
            return ip.Contains(',') ? ip[..ip.IndexOf(',')] : ip;

        await Context.ErrorResponse("No ip found for domain");
        return null;
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Local")]
    private struct Whois
    {
        public bool Success { get; set; }
        public string Type { get; set; }
        public string Continent { get; set; }
        public string Country { get; set; }
        [JsonProperty("country_code")] public string CountryCode { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Postal { get; set; }
        public InternalConnection Connection { get; set; }
        public InternalTimezone Timezone { get; set; }

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
internal partial class LookupCommandExtensions
{
    [GeneratedRegex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b")]
    public static partial Regex IpRegex();
}