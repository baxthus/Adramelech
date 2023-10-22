using System.Diagnostics.CodeAnalysis;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Flurl;

namespace Adramelech.Commands;

[Group("anime", "Anime related commands")]
public class Anime : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [Group("media", "Anime media related commands")]
    public class Media : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        [SlashCommand("image", "Get a random anime image")]
        public async Task ImageAsync([Summary("age-rating", "The age rating of the image")] [Choice("SFW", "SFW")] [Choice("NSFW", "NSFW")] string ageRating)
        {
            var rating = ageRating switch
            {
                "SFW" => "SFW,Questionable,Suggestive",
                "NSFW" => "Borderline,Explicit",
                _ => "big chungus"
            };
            if (rating == "big chungus")
            {
                await Context.ErrorResponse("Invalid age rating");
                return;
            }

            var response = await Utilities.Request<NekosResponse>(new Url("https://api.nekosapi.com")
                    .AppendPathSegments("v2", "images", "random")
                    // ReSharper disable once StringLiteralTypo
                    .SetQueryParam("filter[ageRating.in]", rating.ToLower()),
                Config.Bot.UserAgent);
            if (response.IsInvalid())
            {
                await Context.ErrorResponse("Failed to get image");
                return;
            }

            StringBuilder footer = new();
            
            if (response.Data.Attributes.Source.Url != null)
                footer.AppendLine($"Source: {response.Data.Attributes.Source.Url}");
            
            footer.AppendLine("Powered by nekosapi.com");
            
            var embed = new EmbedBuilder()
                .WithImageUrl(response.Data.Attributes.File)
                .WithFooter(footer.ToString())
                .Build();

            await RespondAsync(embed: embed);
        }

        // Why C# don't have inline structs? Even Go have them
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private struct NekosResponse
        {
            public DataType Data { get; init; }
            
            internal struct DataType
            {
                public AttributesType Attributes { get; init; }
                
                internal struct AttributesType
                {
                    public string File { get; set; }
                    public SourceType Source { get; init; }

                    internal struct SourceType
                    {
                        public string? Url { get; set; }
                    }
                }
            }
        }
    }
}