using Adramelech.Configuration;
using Adramelech.Extensions;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Flurl;

namespace Adramelech.Commands;

public class CepSearch : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("cep", "Search for a CEP (Brazilian postal code)")]
    public async Task CepSearchAsync([Summary("cep", "CEP that you want to search")] string cep)
    {
        await DeferAsync();

        var response = await $"https://brasilapi.com.br/api/cep/v2/{cep}".Request<CepResponse>();
        if (response.IsDefault())
        {
            await Context.SendError("Something went wrong while searching for the CEP", true);
            return;
        }

        if (response.Name is not null)
        {
            var errors = "**Errors:**\n" +
                         $"**Name:** `{response.Name}`\n" +
                         $"**Message:** `{response.Message}`\n" +
                         $"**Type:** `{response.Type}`";

            await Context.SendError(errors, true);
            return;
        }

        var mainField = $"**CEP:** `{response.Cep}`\n" +
                        $"**State:** `{response.State}`\n" +
                        $"**City:** `{response.City}`\n" +
                        $"**Neighborhood:** `{response.Neighborhood}`\n" +
                        $"**Street:** `{response.Street}`\n" +
                        $"**Service:** `{response.Service}`";

        var locationField = $"**Type:** `{response.Location.Type}`\n" +
                            $"**Latitude:** `{response.Location.Coordinates.Latitude.OrElse("N/A")}`\n" +
                            $"**Longitude:** `{response.Location.Coordinates.Longitude.OrElse("N/A")}`";

        var mapsUrl = new Url("https://www.google.com")
            .AppendPathSegments("maps", "search", "/")
            .SetQueryParam("api", 1);

        // If the coordinates are invalid, search for the street, city and state
        mapsUrl.SetQueryParam("query",
            response.Location.Coordinates is { Latitude: null, Longitude: null }
                ? $"{response.Street}, {response.City}, {response.State}"
                : $"{response.Location.Coordinates.Latitude},{response.Location.Coordinates.Longitude}");

        await FollowupAsync(
            embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithTitle("__Adramelech CEP Search__")
                .AddField(":zap: **Main**", mainField, true)
                .AddField(":earth_americas: **Location**", locationField, true)
                .WithFooter("Powered by brasilapi.com.br")
                .Build(),
            components: new ComponentBuilder()
                .WithButton("Open location in Google Maps", url: mapsUrl, style: ButtonStyle.Link,
                    emote: new Emoji("\uD83C\uDF0E"))
                .Build());
    }

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
        public LocationType Location { get; init; }

        internal struct LocationType
        {
            public string Type { get; set; }
            public CoordinatesType Coordinates { get; init; }

            internal struct CoordinatesType
            {
                public string? Latitude { get; set; }
                public string? Longitude { get; set; }
            }
        }
    }
}