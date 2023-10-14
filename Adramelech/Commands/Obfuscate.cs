using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Adramelech.Commands;

public class Obfuscate : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("obfuscate", "Obfuscate a URL")]
    public async Task ObfuscateAsync([Summary("url", "The URL to obfuscate")] string url,
        [Summary("metadata", "Whether to remove the metadata")] bool metadata = false)
    {
        if (!url.StartsWith("http"))
        {
            await Context.ErrorResponse("Invalid URL");
            return;
        }

        var obfuscate = new ObfuscatePost()
        {
            Link = url,
            Generator = "sketchy",
            Metadata = metadata ? "IGNORE" : "PROXY"
        };

        var response = await Utilities.Request<ObfuscatePost, ObfuscateGet>("https://owo.vc/api/v2/link",
            obfuscate, Config.Bot.UserAgent);
        if (response.IsInvalid())
        {
            await Context.ErrorResponse("Error obfuscating URL");
            return;
        }

        var createdAt = DateTimeOffset.Parse(response.CreatedAt);
        var removedMetadata = response.Metadata == "IGNORE" ? "Yes" : "No";

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Obfuscated URL__")
            .AddField(":outbox_tray: **Destination**", $"```{response.Destination}```")
            .AddField(":inbox_tray: **Result**", $"```{response.Id}```")
            .AddField(":wrench: **Method**", $"```{response.Method}```")
            .AddField(":information_source: **Removed the metadata?**", $"```{removedMetadata}```")
            .AddField(":clock1: **Created at**", $"<t:{createdAt.ToUnixTimeSeconds()}>")
            .Build();

        await RespondAsync(embed: embed);
    }

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    private struct ObfuscatePost
    {
        public string Link;
        public string Generator;
        public string Metadata;
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private struct ObfuscateGet
    {
        public string Id { get; set; }
        public string Destination { get; set; }
        public string Method { get; set; }
        public string Metadata { get; set; }
        public string CreatedAt { get; set; }
    }
}