using Adramelech.Database;
using MongoDB.Driver;

namespace Adramelech.Configuration;

public class HttpConfig
{
    private static HttpConfig? _instance;
    public string? ApiToken;
    public string? ApiTokenKey;
    public ulong? FilesChannel;
    public string? BaseUrl;

    private HttpConfig() => FetchFromDatabase();

    public static HttpConfig Instance => _instance ??= new HttpConfig();

    public static void Refresh() => _instance = new HttpConfig();

    private void FetchFromDatabase()
    {
        var apiToken = DatabaseManager.Config.Find(x => x.Key == "ApiToken")
            .FirstOrDefault()
            .Value;
        if (string.IsNullOrEmpty(apiToken))
            apiToken = null;

        var apiTokenKey = DatabaseManager.Config.Find(x => x.Key == "ApiTokenKey")
            .FirstOrDefault()
            .Value;
        if (string.IsNullOrEmpty(apiTokenKey))
            apiTokenKey = null;

        var filesChannelId = DatabaseManager.Config.Find(x => x.Key == "FilesChannelId")
            .FirstOrDefault()
            .Value;
        if (string.IsNullOrEmpty(filesChannelId))
            filesChannelId = null;

        var baseUrl = DatabaseManager.Config.Find(x => x.Key == "BaseUrl")
            .FirstOrDefault()
            .Value;
        if (string.IsNullOrEmpty(baseUrl))
            baseUrl = null;

#if DEBUG
        baseUrl ??= "http://localhost:8000";
#endif

        ApiToken = apiToken;
        ApiTokenKey = apiTokenKey;
        FilesChannel = filesChannelId is null ? null : ulong.Parse(filesChannelId);
        BaseUrl = baseUrl;
    }
}