using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog;

namespace Adramelech;

public static class Database
{
    private static MongoClient Client { get; set; } = null!;
    private static IMongoDatabase Db { get; set; } = null!;
    public static IMongoCollection<ConfigSchema> Config { get; private set; } = null!;

    public static void CreateConnection()
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            Log.Fatal("MongoDB connection string not found in environment variables.");
            Environment.Exit(1);
        }

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);

        // Using a try catch here because a lot of things can go wrong and I don't want to deal with them (basically my life).
        try
        {
            Client = new MongoClient(settings);
            Db = Client.GetDatabase("adramelech");
            Config = Db.GetCollection<ConfigSchema>("config");
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to connect to MongoDB.");
            Environment.Exit(1);
        }

        // Setup camelCase convention, because the C# and MongoDB naming conventions are different.
        var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camelCase", camelCaseConvention, _ => true);

        Log.Debug("Opened database connection, Database: {Database}", Db.DatabaseNamespace.DatabaseName);
    }


    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public struct ConfigSchema
    {
        // ReSharper disable once UnusedMember.Global
        public ObjectId Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}