using Adramelech.Configuration;
using Adramelech.Extensions;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Adramelech.Commands;

public class Obfuscate : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("obfuscate", "Obfuscate a URL")]
    public async Task ObfuscateAsync([Summary("url", "The URL to obfuscate")] string url,
        [Summary("metadata", "Whether to remove the metadata")]
        bool metadata = false)
    {
        await DeferAsync();

        if (!url.StartsWith("http"))
        {
            await Context.ErrorResponse("Invalid URL", true);
            return;
        }

        var response = await $"https://owo.vc/api/v2/link".Request<ObfuscateData, ObfuscateResponse>(
            new ObfuscateData
            {
                Link = url,
                Generator = "sketchy",
                Metadata = metadata ? "IGNORE" : "PROXY"
            },
            OtherConfig.UserAgent);
        if (response.IsDefault())
        {
            await Context.ErrorResponse("Error obfuscating URL", true);
            return;
        }

        var createdAt = DateTimeOffset.Parse(response.CreatedAt);
        var removedMetadata = response.Metadata == "IGNORE" ? "Yes" : "No";

        await FollowupAsync(embed: new EmbedBuilder()
            .WithColor(BotConfig.EmbedColor)
            .WithTitle("__Obfuscated URL__")
            .AddField(":outbox_tray: **Destination**", $"```{response.Destination}```")
            .AddField(":inbox_tray: **Result**", $"```{response.Id}```")
            .AddField(":wrench: **Method**", $"```{response.Method}```")
            .AddField(":information_source: **Removed the metadata?**", $"```{removedMetadata}```")
            .AddField(":clock1: **Created at**", $"<t:{createdAt.ToUnixTimeSeconds()}>")
            .WithFooter("Powered by owo.vc")
            .Build());
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    private struct ObfuscateData
    {
        public string Link;
        public string Generator;
        public string Metadata;
    }

    private struct ObfuscateResponse
    {
        public string Id { get; set; }
        public string Destination { get; set; }
        public string Method { get; set; }
        public string Metadata { get; set; }
        public string CreatedAt { get; set; }
    }
}