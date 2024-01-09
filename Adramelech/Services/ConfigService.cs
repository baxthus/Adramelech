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
    public int ApiPort { get; private set; }
    public int BridgePort { get; private set; }
    public string BaseUrl { get; private set; } = null!;
    public ulong? FilesChannel { get; private set; }

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
            ? token.Value!.First().Value
            : throw new Exception("Failed to get token from database.", token.Exception);

        var apiPort = await _dbService.GetConfigAsync("ApiPort");
        ApiPort = apiPort.Success
            ? int.Parse(apiPort.Value!.FirstOrDefault().Value ?? "5000")
            : throw new Exception("Failed to get port from database.", apiPort.Exception);

        var bridgePort = await _dbService.GetConfigAsync("BridgePort");
        BridgePort = bridgePort.Success
            ? int.Parse(bridgePort.Value!.FirstOrDefault().Value ?? "8000")
            : throw new Exception("Failed to get port from database.", bridgePort.Exception);

        var baseUrl = await _dbService.GetConfigAsync("BaseUrl");
        BaseUrl = baseUrl.Success
            // If null, defaults to bridge server (debug purposes)
            ? baseUrl.Value!.FirstOrDefault().Value ?? $"http://localhost:{BridgePort}"
            : throw new Exception("Failed to get base url from database.", baseUrl.Exception);

        var filesChannelId = await _dbService.GetConfigAsync("FilesChannelId");
        FilesChannel = filesChannelId.Success
            ? filesChannelId.Value!.FirstOrDefault().Value is null
                ? null
                : ulong.Parse(filesChannelId.Value!.FirstOrDefault().Value)
            : throw new Exception("Failed to get files channel id from database.", filesChannelId.Exception);

        var openWeatherKey = await _dbService.GetConfigAsync("OpenWeatherKey");
        OpenWeatherKey = openWeatherKey.Success
            ? openWeatherKey.Value!.First().Value
            : throw new Exception("Failed to get open weather key from database.", openWeatherKey.Exception);

        var feedbackWebhook = await _dbService.GetConfigAsync("FeedbackWebhook");
        FeedbackWebhook = feedbackWebhook.Success
            ? feedbackWebhook.Value!.First().Value
            : throw new Exception("Failed to get feedback webhook from database.", feedbackWebhook.Exception);
    }
}