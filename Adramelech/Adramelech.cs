using System.Reflection;
using Adramelech.Configuration;
using Adramelech.Database;
using Adramelech.Http;
using Adramelech.Logging;
using Adramelech.Services;
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
        Log.Logger = Loggers.Default;

        DatabaseManager.Connect();

        var services = ConfigureServices();

        _client = services.GetRequiredService<DiscordSocketClient>();
        _interactionService = services.GetRequiredService<InteractionService>();

        _client.Log += LogAsync;
        _interactionService.Log += LogAsync;

        await _client.SetActivityAsync(BotConfig.Activity);

        await _client.LoginAsync(TokenType.Bot, BotConfig.Instance.Token);
        await _client.StartAsync();

        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        services.GetRequiredService<InteractionHandler>().Initialize();
        services.GetRequiredService<CommandHandler>().Initialize();
        services.GetRequiredService<ReadyHandler>().Initialize();
        await services.GetRequiredService<HttpServer>().InitializeAsync();

        await Task.Delay(-1);
    }

    private static ServiceProvider ConfigureServices() => new ServiceCollection()
        .AddSingleton<DiscordSocketClient>()
        .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
        .AddSingleton<InteractionHandler>()
        .AddSingleton<CommandHandler>()
        .AddSingleton<ReadyHandler>()
        .AddSingleton<HttpServer>()
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