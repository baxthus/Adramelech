using Adramelech.Database;
using Discord;
using MongoDB.Driver;

namespace Adramelech.Configuration;

public class BotConfig
{
    private static BotConfig? _instance;
    public string Token = null!;
    public static readonly Color EmbedColor = new(203, 166, 247);
    public static readonly Game Activity = new("with your mom <3");

    private BotConfig() => FetchFromDatabase();

    public static BotConfig Instance => _instance ??= new BotConfig();

    private void FetchFromDatabase()
    {
        var botToken = DatabaseManager.Config.Find(x => x.Key == "Token")
            .FirstOrDefault()
            .Value;
        if (string.IsNullOrEmpty(botToken))
            throw new Exception("Bot token not found in database");

        Token = botToken;
    }
}