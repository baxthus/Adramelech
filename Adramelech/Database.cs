using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog;

namespace Adramelech;

public static class Database
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    // ReSharper disable once ClassNeverInstantiated.Local
    public class ConfigSchema
    {
        public ObjectId Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    
    private static MongoClient Client { get; set; } = null!;
    private static IMongoDatabase Db { get; set; } = null!;
    public static IMongoCollection<ConfigSchema> Config { get; set; } = null!;

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
        
        var camelCaseConvention = new ConventionPack {new CamelCaseElementNameConvention()};
        ConventionRegistry.Register("camelCase", camelCaseConvention, t => true);
        
        Log.Debug("Opened database connection, Database: {Database}", Db.DatabaseNamespace.DatabaseName);
    }
}