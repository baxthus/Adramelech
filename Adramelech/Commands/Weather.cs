﻿using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Flurl;
using Newtonsoft.Json;

namespace Adramelech.Commands;

public class Weather : InteractionModuleBase<SocketInteractionContext>
{
    private const string OpenWeatherUrl = "https://api.openweathermap.org";
    
    [SlashCommand("weather", "Get the weather for a location")]
    public async Task WeatherAsync(
        [Summary("city", "The city to get the weather for")]
        string city,
        [Summary("country", "The country to get the weather for")]
        string country)
    {
        // Get coordinates
        var coordinates = await Utilities.Request<OpenWeatherGeo[]>(new Url(OpenWeatherUrl)
            .AppendPathSegments("geo", "1.0", "direct")
            .SetQueryParam("q", $"{city},{country}")
            .SetQueryParam("appid", Config.Bot.OpenWeatherKey));
        if (coordinates.IsInvalid())
        {
            await Context.ErrorResponse("Error getting coordinates");
            return;
        }
        
        // Get weather
        var weather = await Utilities.Request<OpenWeather>(new Url(OpenWeatherUrl)
            .AppendPathSegments("data", "2.5", "weather")
            .SetQueryParam("lat", coordinates?[0].Lat)
            .SetQueryParam("lon", coordinates?[0].Lon)
            .SetQueryParam("appid", Config.Bot.OpenWeatherKey)
            .SetQueryParam("units", "metric")
            .SetQueryParam("lang", "en"));
        if (weather.IsInvalid())
        {
            await Context.ErrorResponse("Error getting weather");
            return;
        }
        
        var mainField = $"**Temperature:** {weather.Main.Temp}ºC\n" +
                        $"**Feels Like:** {weather.Main.FeelsLike}ºC\n" +
                        $"**Minimum Temperature:** {weather.Main.TempMin}ºC\n" +
                        $"**Maximum Temperature:** {weather.Main.TempMax}ºC\n" +
                        $"**Pressure:** {weather.Main.Pressure}hPa\n" +
                        $"**Humidity:** {weather.Main.Humidity}%\n" +
                        $"**Sea Level:** {weather.Main.SeaLevel}hPa\n" +
                        $"**Ground Level:** {weather.Main.GroundLevel}hPa";
        
        var weatherField = $"**Main:** {weather.Weather[0].Main}\n" +
                           $"**Description:** {weather.Weather[0].Description}";
        
        var windField = $"**Speed:** {weather.Wind.Speed}m/s\n" +
                        $"**Direction:** {weather.Wind.Deg}º\n" +
                        $"**Gust:** {weather.Wind.Gust}m/s";

        var place = weather.Name.IsInvalid() ? string.Empty : $"in {weather.Name}";

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle($"__Weather {place}__")
            .AddField(":zap: **Main**", mainField)
            .AddField(":cloud: **Weather**", weatherField)
            .AddField(":dash: **Wind**", windField)
            .WithFooter("Powered by openweathermap.org")
            .Build();

        await RespondAsync(embed: embed);
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private struct OpenWeatherGeo
    {
        public string Lat { get; set; }
        public string Lon { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private struct OpenWeather
    {
       public WeatherType[] Weather { get; set; }
       public MainType Main { get; init; }
       public WindType Wind { get; init; }
       public string? Name { get; set; }
        
        internal struct WeatherType
        {
            public string Main { get; set; }
            public string Description { get; set; }
        }

        internal struct MainType
        {
            public double Temp { get; set; }
            [JsonProperty("feels_like")] public double FeelsLike { get; set; }
            [JsonProperty("temp_min")] public double TempMin { get; set; }
            [JsonProperty("temp_max")] public double TempMax { get; set; }
            public int Pressure { get; set; }
            public int Humidity { get; set; }
            [JsonProperty("sea_level")] public int SeaLevel { get; set; }
            [JsonProperty("grnd_level")] public int GroundLevel { get; set; }
        }

        internal struct WindType
        {
            public double Speed { get; set; }
            public int Deg { get; set; }
            public double Gust { get; set; }
        }
    }
}