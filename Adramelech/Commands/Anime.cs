using System.Text;
using Adramelech.Configuration;
using Adramelech.Extensions;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Flurl;
using Newtonsoft.Json;

namespace Adramelech.Commands;

[Group("anime", "Anime related commands")]
public class Anime : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [Group("media", "Anime media related commands")]
    public class Media : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        [SlashCommand("image", "Get a random anime image")]
        public async Task ImageAsync(
            [Summary("age-rating", "The age rating of the image")] [Choice("SFW", "SFW")] [Choice("NSFW", "NSFW")]
            string ageRating)
        {
            await DeferAsync();

            string[] rating;

            switch (ageRating)
            {
                case "SFW":
                    rating = new[] { "safe", "questionable" };
                    break;
                case "NSFW":
                    if (Context.Channel is ITextChannel { IsNsfw: false })
                    {
                        await Context.ErrorResponse("This channel is not NSFW", true);
                        return;
                    }

                    rating = new[] { "borderline", "explicit" };
                    break;
                default:
                    await Context.ErrorResponse("Invalid age rating");
                    return;
            }

            var url = new Url("https://api.nekosapi.com")
                .AppendPathSegments("v3", "images", "random")
                .SetQueryParam("limit", 1)
                .SetQueryParam("rating", rating)
                .ToString()!;

            var response = await url.Request<NekosApiResponse>(OtherConfig.UserAgent);
            if (response.IsDefault())
            {
                await Context.ErrorResponse("Failed to get image", true);
                return;
            }

            var data = response.Items.First();

            StringBuilder footer = new();

            if (data.Source is not null)
                footer.AppendLine($"Source: {data.Source}");

            footer.Append("Powered by nekosapi.com");

            await FollowupAsync(embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithImageUrl(data.ImageUrl)
                .WithFooter(footer.ToString())
                .Build());
        }

        private struct NekosApiResponse
        {
            public Item[] Items { get; set; }

            internal struct Item
            {
                [JsonProperty("image_url")] public string ImageUrl { get; set; }
                public string? Source { get; set; }
            }
        }
    }
}