using Adramelech.Http.Schemas;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog;

namespace Adramelech.Database;

public static class DatabaseManager
{
    private static MongoClient? Client { get; set; }
    private static IMongoDatabase? AdramelechDb { get; set; }
    private static IMongoDatabase? GeneralDb { get; set; }
    public static IMongoCollection<ConfigSchema> Config { get; private set; } = null!;
    public static IMongoCollection<MusicSchema> Music { get; private set; } = null!;
    public static IMongoCollection<FileSchema> Files { get; private set; } = null!;

    public static void Connect()
    {
        // Register camelCase convention for MongoDB
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
            AdramelechDb = Client.GetDatabase("adramelech");
            GeneralDb = Client.GetDatabase("general");
            Config = AdramelechDb.GetCollection<ConfigSchema>("config");
            Music = GeneralDb.GetCollection<MusicSchema>("music");
            Files = AdramelechDb.GetCollection<FileSchema>("files");
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to connect to MongoDB.");
            Environment.Exit(1);
        }

        Log.Debug("Opened database connection, Database: {Database}", AdramelechDb.DatabaseNamespace.DatabaseName);
        Log.Debug("Opened database connection, Database: {Database}", GeneralDb.DatabaseNamespace.DatabaseName);
    }
}