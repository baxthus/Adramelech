using System.Reflection;
using Adramelech.Events;
using Adramelech.Http;
using Adramelech.Logging;
using Adramelech.Services;
using Adramelech.Tools;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Adramelech;

public class Adramelech
{
    public static Task Main() => new Adramelech().MainAsync();

    private DiscordSocketClient _client = null!;
    private InteractionService _interactionService = null!;

    private async Task MainAsync()
    {
#if DEBUG
        // Load before anything else so we can use it in the DotEnv
        // Cannot load before in normal occasions because of the Sentry DSN
        Log.Logger = Loggers.Default;
#endif

        DotEnv.Load();

        Log.Logger = Loggers.Default;

        var services = ConfigureServices();

        _client = services.GetRequiredService<DiscordSocketClient>();
        _interactionService = services.GetRequiredService<InteractionService>();

        _client.Log += LogAsync;
        _interactionService.Log += LogAsync;

        await _client.SetActivityAsync(ConfigService.Activity);

        await _client.LoginAsync(TokenType.Bot, services.GetRequiredService<ConfigService>().Token);
        await _client.StartAsync();

        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        services.GetRequiredService<InteractionHandler>().Initialize();
        services.GetRequiredService<CommandHandler>().Initialize();
        services.GetRequiredService<ReadyHandler>().Initialize();
        services.GetRequiredService<HttpWrapper>().Initialize();

        await Task.Delay(-1);
    }

    private static ServiceProvider ConfigureServices() => new ServiceCollection()
        // Discord
        .AddSingleton<DiscordSocketClient>()
        .AddSingleton<InteractionService>()
        // Configuration
        .AddSingleton<DatabaseService>()
        .AddSingleton<ConfigService>()
        // Handlers
        .AddSingleton<InteractionHandler>()
        .AddSingleton<CommandHandler>()
        .AddSingleton<ReadyHandler>()
        // Others
        .AddSingleton<HttpWrapper>()
        .BuildServiceProvider();

    private static async Task LogAsync(LogMessage msg)
    {
        var severity = msg.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };

        Log.Write(severity, msg.Exception, "[{Source}] {Message}", msg.Source, msg.Message);

        await Task.CompletedTask;
    }
}