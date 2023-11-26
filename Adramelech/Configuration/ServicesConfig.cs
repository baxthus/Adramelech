using Adramelech.Database;
using MongoDB.Driver;
using Serilog;

namespace Adramelech.Configuration;

public class ServicesConfig
{
    private static ServicesConfig? _instance;
    public string OpenWeatherKey = null!;
    public string FeedbackWebhook = null!;

    private ServicesConfig() => FetchFromDatabase();

    public static ServicesConfig Instance => _instance ??= new ServicesConfig();

    private void FetchFromDatabase()
    {
        var openWeatherKey = DatabaseManager.Config.Find(x => x.Key == "OpenWeatherKey")
            .FirstOrDefault()
            .Value;
        if (string.IsNullOrEmpty(openWeatherKey))
        {
            Log.Fatal("OpenWeatherKey not found in database.");
            Environment.Exit(1);
        }

        var feedbackWebhook = DatabaseManager.Config.Find(x => x.Key == "FeedbackWebhook")
            .FirstOrDefault()
            .Value;
        if (string.IsNullOrEmpty(feedbackWebhook))
        {
            Log.Fatal("FeedbackWebhook not found in database.");
            Environment.Exit(1);
        }

        OpenWeatherKey = openWeatherKey;
        FeedbackWebhook = feedbackWebhook;
    }
}