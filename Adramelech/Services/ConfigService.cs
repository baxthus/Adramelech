using Discord;

namespace Adramelech.Services;

public class ConfigService
{
    private readonly DatabaseService _dbService;

    // Bot
    public string Token { get; private set; } = null!;
    public static readonly Color EmbedColor = new(203, 166, 247);
    public static readonly Game Activity = new("with your mom <3");

    // Http
    public int Port { get; private set; } = 5050;
    public string BaseUrl { get; private set; } = null!;
    public ulong? FilesChannel { get; private set; }
    public string? ApiToken { get; private set; }
    public string? ApiTokenKey { get; private set; }


    // Services
    public string OpenWeatherKey { get; private set; } = null!;
    public string FeedbackWebhook { get; private set; } = null!;

    // Other
    public const string UserAgent = "Adramelech (By baxthus)";

    public ConfigService(DatabaseService dbService)
    {
        _dbService = dbService;
        LoadAsync().Wait();
    }

    public async Task ReloadAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        var token = await _dbService.GetConfigAsync("Token");
        Token = token.Success
            ? token.Value!.Value
            : throw new Exception("Failed to get token from database.", token.Exception);

        Port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "5050");

        var baseUrl = await _dbService.GetConfigAsync("BaseUrl");
        BaseUrl = baseUrl.Success
            // If null, defaults to localhost
            ? baseUrl.Value?.Value ?? $"http://localhost:{Port}"
            : throw new Exception("Failed to get base url from database.", baseUrl.Exception);

        var filesChannelId = await _dbService.GetConfigAsync("FilesChannelId");
        FilesChannel = filesChannelId.Success
            ? filesChannelId.Value?.Value is null
                ? null
                : ulong.Parse(filesChannelId.Value.Value)
            : throw new Exception("Failed to get files channel id from database.", filesChannelId.Exception);

        var apiToken = await _dbService.GetConfigAsync("ApiToken");
        ApiToken = apiToken.Success
            ? apiToken.Value?.Value
            : throw new Exception("Failed to get api token from database.", apiToken.Exception);

        var apiTokenKey = await _dbService.GetConfigAsync("ApiTokenKey");
        ApiTokenKey = apiTokenKey.Success
            ? apiTokenKey.Value?.Value
            : throw new Exception("Failed to get api token key from database.", apiTokenKey.Exception);

        var openWeatherKey = await _dbService.GetConfigAsync("OpenWeatherKey");
        OpenWeatherKey = openWeatherKey.Success
            ? openWeatherKey.Value!.Value
            : throw new Exception("Failed to get open weather key from database.", openWeatherKey.Exception);

        var feedbackWebhook = await _dbService.GetConfigAsync("FeedbackWebhook");
        FeedbackWebhook = feedbackWebhook.Success
            ? feedbackWebhook.Value!.Value
            : throw new Exception("Failed to get feedback webhook from database.", feedbackWebhook.Exception);
    }
}