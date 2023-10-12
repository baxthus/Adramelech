using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Flurl;

namespace Adramelech.Commands;

public class CepSearchCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("cep", "Search for a CEP (Brazilian postal code)")]
    public async Task CepSearch([Summary("cep", "CEP that you want to search")] string cep)
    {
        var response = await Utilities.Request<CepResponse>($"https://brasilapi.com.br/api/cep/v2/{cep}");
        if (response.IsInvalid())
        {
            await Context.ErrorResponse("Something went wrong while searching for the CEP");
        }

        if (!response.Name.IsInvalid())
        {
            var errors = "**Errors:**\n" +
                         $"**Name:** `{response.Name}`\n" +
                         $"**Message:** `{response.Message}`\n" +
                         $"**Type:** `{response.Type}`";

            await Context.ErrorResponse(errors);
            return;
        }

        var mainField = $"**CEP:** `{response.Cep}`\n" +
                        $"**State:** `{response.State}`\n" +
                        $"**City:** `{response.City}`\n" +
                        $"**Neighborhood:** `{response.Neighborhood}`\n" +
                        $"**Street:** `{response.Street}`\n" +
                        $"**Service:** `{response.Service}`";

        var locationField = $"**Type:** `{response.Location.Type}`\n" +
                            $"**Latitude:** `{response.Location.Coordinates.Latitude ?? "N/A"}`\n" +
                            $"**Longitude:** `{response.Location.Coordinates.Longitude ?? "N/A"}`";

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Adramelech CEP Search__")
            .AddField(":zap: **Main**", mainField, true)
            .AddField(":earth_americas: **Location**", locationField, true)
            .WithFooter("Powered by brasilapi.com.br")
            .Build();

        // If the coordinates are invalid, we don't need to add the button
        if (response.Location.Coordinates.Latitude.IsInvalid() || response.Location.Coordinates.Longitude.IsInvalid())
        {
            await RespondAsync(embed: embed);
            return;
        }

        var mapsUrl = new Url("https://www.google.com")
            .AppendPathSegments("maps", "search", "/")
            .SetQueryParam("api", 1)
            .SetQueryParam("query",
                $"{response.Location.Coordinates.Latitude},{response.Location.Coordinates.Longitude}");

        var button = new ComponentBuilder()
            .WithButton("Open location in Google Maps", url: mapsUrl, style: ButtonStyle.Link,
                // Emoji is :earth_americas:
                emote: new Emoji("\uD83C\uDF0E"))
            .Build();

        await RespondAsync(embed: embed, components: button);
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Local")]
    private struct CepResponse
    {
        public string? Name { get; set; }
        public string? Message { get; set; }
        public string? Type { get; set; }
        public string Cep { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Neighborhood { get; set; }
        public string Street { get; set; }
        public string Service { get; set; }
        public LocationStruct Location { get; set; }

        internal struct LocationStruct
        {
            public string Type { get; set; }
            public CoordinatesStruct Coordinates { get; set; }

            internal struct CoordinatesStruct
            {
                public string? Latitude { get; set; }
                public string? Longitude { get; set; }
            }
        }
    }
}