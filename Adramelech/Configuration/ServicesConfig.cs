using Adramelech.Database;
using MongoDB.Driver;

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
            throw new Exception("OpenWeatherKey not found in database.");

        var feedbackWebhook = DatabaseManager.Config.Find(x => x.Key == "FeedbackWebhook")
            .FirstOrDefault()
            .Value;
        if (string.IsNullOrEmpty(feedbackWebhook))
            throw new Exception("FeedbackWebhook not found in database.");

        OpenWeatherKey = openWeatherKey;
        FeedbackWebhook = feedbackWebhook;
    }
}