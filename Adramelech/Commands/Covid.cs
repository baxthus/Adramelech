using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Covid : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    private const string BaseUrl = "https://disease.sh/v3/covid-19";

    [SlashCommand("covid", "Get covid stats")]
    public async Task CovidAsync(
        [Summary("country", "Country to get stats from (can be 'worldwide')")]
        string country = "worldwide")
    {
        var response = country.ToLower() == "worldwide"
            ? await Utilities.Request<CovidResponse>(BaseUrl + "/all")
            : await Utilities.Request<CovidResponse>(BaseUrl + $"/countries/{country}");
        if (response.IsInvalid())
        {
            await Context.ErrorResponse("Error getting covid stats");
            return;
        }

        if (!response.Message.IsInvalid())
        {
            await Context.ErrorResponse($"`{response.Message}`");
            return;
        }

        var local = country.ToLower() == "worldwide" ? country.ToLower() : response.Country;

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle($"__Covid Stats for {local}__")
            .WithDescription($"Cases: {response.Cases}\n" +
                             $"Today Cases: {response.TodayCases}\n" +
                             $"Deaths: {response.Deaths}\n" +
                             $"Today Deaths: {response.TodayDeaths}\n" +
                             $"Recovered: {response.Recovered}\n" +
                             $"Today Recovered: {response.TodayRecovered}")
            .WithFooter($"Powered by disease.sh")
            .Build();

        await RespondAsync(embed: embed);
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private struct CovidResponse
    {
        public string? Message { get; set; }
        public string? Country { get; set; }
        public string Cases { get; set; }
        public string TodayCases { get; set; }
        public string Deaths { get; set; }
        public string TodayDeaths { get; set; }
        public string Recovered { get; set; }
        public string TodayRecovered { get; set; }
    }
}