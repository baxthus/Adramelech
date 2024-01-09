using Adramelech.Common;
using Adramelech.Database;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog;

namespace Adramelech.Services;

public class DatabaseService
{
    private readonly MongoClient? _client;
    private readonly IMongoDatabase? _adramelechDb;
    private readonly IMongoDatabase? _generalDb;
    private readonly IMongoCollection<ConfigSchema> _config;
    public readonly IMongoCollection<MusicSchema> Music;

    public DatabaseService()
    {
        // Register camelCase convention for MongoDB
        var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camelCase", camelCaseConvention, _ => true);

        var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("MONGODB_CONNECTION_STRING environment variable is not set.");

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);

#if DEBUG
        // Workaround for my shitty computer
        settings.AllowInsecureTls = true;
#endif

        // Using a try catch here because a lot of things can go wrong and I don't want to deal with them (basically my life)
        try
        {
            _client = new MongoClient(settings);
            _adramelechDb = _client.GetDatabase("adramelech");
            _generalDb = _client.GetDatabase("general");
            _config = _adramelechDb.GetCollection<ConfigSchema>("config");
            Music = _generalDb.GetCollection<MusicSchema>("music");
        }
        catch (Exception e)
        {
            throw new Exception("Failed to connect to MongoDB.", e);
        }

        Log.Debug("Opened database connection, Database: {Database}", _adramelechDb.DatabaseNamespace.DatabaseName);
        Log.Debug("Opened database connection, Database: {Database}", _generalDb.DatabaseNamespace.DatabaseName);
    }

    public Result<List<ConfigSchema>> GetConfig(string key)
    {
        try
        {
            var filter = Builders<ConfigSchema>.Filter.Eq(schema => schema.Key, key);
            var result = _config.Find(filter);
            return result.ToList();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<Result<List<ConfigSchema>>> GetConfigAsync(string key)
    {
        try
        {
            var filter = Builders<ConfigSchema>.Filter.Eq(schema => schema.Key, key);
            var result = _config.Find(filter);
            return await result.ToListAsync();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<Result<bool>> ConfigExistsAsync(string key)
    {
        try
        {
            var filter = Builders<ConfigSchema>.Filter.Eq(schema => schema.Key, key);
            var result = await _config.FindAsync(filter);
            return await result.AnyAsync();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<Result> InsertConfigAsync(ConfigSchema config)
    {
        try
        {
            await _config.InsertOneAsync(config);
            return true;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<Result<UpdateResult>> UpdateConfigAsync(ConfigSchema config, string? newKey = null)
    {
        try
        {
            var filter = Builders<ConfigSchema>.Filter.Eq(schema => schema.Key, config.Key);
            var update = Builders<ConfigSchema>.Update.Set(schema => schema.Value, config.Value);
            if (newKey is not null)
                update = update.Set(schema => schema.Key, newKey);
            var result = await _config.UpdateOneAsync(filter, update);
            return result;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<Result> DeleteConfigAsync(string key)
    {
        try
        {
            var filter = Builders<ConfigSchema>.Filter.Eq(schema => schema.Key, key);
            await _config.DeleteOneAsync(filter);
            return true;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}