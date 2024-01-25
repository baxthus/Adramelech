using Adramelech.Common;
using Adramelech.Models;
using Adramelech.Utilities;
using Postgrest;
using Serilog;
using Supabase.Interfaces;
using Supabase.Realtime;

namespace Adramelech.Services;

public class DatabaseService
{
    public readonly Supabase.Client Client;
    private readonly ISupabaseTable<ConfigModel, RealtimeChannel> _config;
    public readonly ISupabaseTable<FileModel, RealtimeChannel> Files;

    public DatabaseService()
    {
        var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
        if (string.IsNullOrEmpty(url))
            throw new Exception("SUPABASE_URL environment variable is not set.");
        var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");
        if (string.IsNullOrEmpty(key))
            throw new Exception("SUPABASE_KEY environment variable is not set.");

        Client = new Supabase.Client(url, key);

        if (ErrorUtils.Try(Client.InitializeAsync().Wait) is { Success: false } init)
            throw new Exception("Failed to initialize Supabase client.", init.Exception);

        _config = Client.From<ConfigModel>();
        Files = Client.From<FileModel>();

        Log.Debug("Database service initialized.");
    }

    public async Task<Result<ConfigModel?>> GetConfigAsync(string key)
    {
        var result = await ErrorUtils.TryAsync(() => _config.Where(x => x.Key == key).Single());
        return result.Success ? result.Value : new Exception("Failed to get config.", result.Exception);
    }

    public async Task<Result<bool>> ConfigExistsAsync(string key)
    {
        var result = await ErrorUtils.TryAsync(() => _config.Where(x => x.Key == key).Single());
        return result.Success
            ? result.Value is not null
            : new Exception("Failed to get config.", result.Exception);
    }

    public async Task<Result> InsertConfigAsync(ConfigModel config)
    {
        var result = await ErrorUtils.TryAsync(() => _config.Insert(config));
        return result.Success ? true : new Exception("Failed to insert config.", result.Exception);
    }

    public async Task<Result<ConfigModel>> UpdateConfigAsync(ConfigModel config)
    {
        try
        {
            var model = await _config.Where(x => x.Key == config.Key).Single();
            if (model is null)
                throw new Exception("Failed to get config.");

            var result = await _config.Update(config);
            return result.Model ?? throw new Exception("Failed to update config.");
        }
        catch (Exception e)
        {
            return new Exception("Failed to update config.", e);
        }
    }

    public async Task<Result<ConfigModel>> UpsertConfigAsync(ConfigModel config)
    {
        var result = await ErrorUtils.TryAsync(() => _config.Upsert(config, new QueryOptions { Upsert = true }));
        return result.Success ? result.Value!.Model : new Exception("Failed to upsert config.", result.Exception);
    }

    public async Task<Result> DeleteConfigAsync(string key)
    {
        var result = await ErrorUtils.TryAsync(() => _config.Where(x => x.Key == key).Delete());
        return result.Success ? true : new Exception("Failed to delete config.", result.Exception);
    }
}