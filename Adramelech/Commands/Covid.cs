using Adramelech.Configuration;
using Adramelech.Extensions;
using Adramelech.Utilities;
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
        await DeferAsync();

        var response = country.ToLower() == "worldwide"
            ? await $"{BaseUrl}/all".Request<CovidResponse>()
            : await $"{BaseUrl}/countries/{country}".Request<CovidResponse>();
        if (response.IsDefault())
        {
            await Context.ErrorResponse("Error getting covid stats", true);
            return;
        }

        if (!response.Message.IsNullOrEmpty())
        {
            await Context.ErrorResponse($"`{response.Message}`", true);
            return;
        }

        var local = country.ToLower() == "worldwide" ? country.ToLower() : response.Country;

        await FollowupAsync(embed: new EmbedBuilder()
            .WithColor(BotConfig.EmbedColor)
            .WithTitle($"__Covid Stats for {local}__")
            .WithDescription($"Cases: {response.Cases}\n" +
                             $"Today Cases: {response.TodayCases}\n" +
                             $"Deaths: {response.Deaths}\n" +
                             $"Today Deaths: {response.TodayDeaths}\n" +
                             $"Recovered: {response.Recovered}\n" +
                             $"Today Recovered: {response.TodayRecovered}")
            .WithFooter($"Powered by disease.sh")
            .Build());
    }

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