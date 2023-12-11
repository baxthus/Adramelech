using Adramelech.Database;
using MongoDB.Driver;

namespace Adramelech.Configuration;

public class HttpConfig
{
    private static HttpConfig? _instance;
    public string? ApiToken;
    public ulong? FilesChannel;

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

        var filesChannelId = DatabaseManager.Config.Find(x => x.Key == "FilesChannelId")
            .FirstOrDefault()
            .Value;
        if (string.IsNullOrEmpty(filesChannelId))
            filesChannelId = null;

        ApiToken = apiToken;
        FilesChannel = filesChannelId is null ? null : ulong.Parse(filesChannelId);
    }
}