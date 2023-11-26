using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog;

namespace Adramelech.Database;

public static class DatabaseManager
{
    private static MongoClient Client { get; set; } = null!;
    private static IMongoDatabase ConfigDb { get; set; } = null!;
    private static IMongoDatabase GeneralDb { get; set; } = null!;
    public static IMongoCollection<ConfigSchema> Config { get; private set; } = null!;
    public static IMongoCollection<MusicSchema> Music { get; private set; } = null!;

    public static void Connect()
    {
        // Setup camelCase convention, because the C# and MongoDB naming conventions are different
        var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camelCase", camelCaseConvention, _ => true);

        var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            Log.Fatal("MongoDB connection string not found in environment variables.");
            Environment.Exit(1);
        }

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
#if DEBUG
        // Workaround for my shitty computer
        settings.AllowInsecureTls = true;
#endif

        // Using a try catch here because a lot of things can go wrong and I don't want to deal with them (basically my life)
        try
        {
            Client = new MongoClient(settings);
            ConfigDb = Client.GetDatabase("adramelech");
            GeneralDb = Client.GetDatabase("general");
            Config = ConfigDb.GetCollection<ConfigSchema>("config");
            Music = GeneralDb.GetCollection<MusicSchema>("music");
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to connect to MongoDB.");
            Environment.Exit(1);
        }

        Log.Debug("Opened database connection, Database: {Database}", ConfigDb.DatabaseNamespace.DatabaseName);
    }
}