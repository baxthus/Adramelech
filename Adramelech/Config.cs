using Discord;
using MongoDB.Driver;
using Serilog;

namespace Adramelech;

public static class Config
{
    public static void Verify()
    {
        if (string.IsNullOrEmpty(Bot.Token) ||
            string.IsNullOrEmpty(Bot.OpenWeatherKey) ||
            string.IsNullOrEmpty(Bot.FeedbackWebhook))
        {
            Log.Fatal("Data not found in database.");
            Environment.Exit(1);
        }
        
        Log.Debug("All data found in database.");
    }
    
    public abstract class Bot
    {
        public static readonly string Token = Database.Config.Find(x => x.Key == "Token").FirstOrDefault().Value;
        public static readonly string OpenWeatherKey = Database.Config.Find(x => x.Key == "OpenWeatherKey").FirstOrDefault().Value;
        public static readonly string FeedbackWebhook = Database.Config.Find(x => x.Key == "FeedbackWebhook").FirstOrDefault().Value;
        public static readonly Color EmbedColor = new(203, 166, 247);
        public static readonly Game Activity = new("with your mom <3");
        public const string UserAgent = "Adramelech (By @baxthus)";
        
    }
}