using System.Reflection;
using Adramelech.Configuration;
using Adramelech.Database;
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
    private InteractionService _commands = null!;

    private async Task MainAsync()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        DatabaseManager.Connect();

        var services = ConfigureServices();

        _client = services.GetRequiredService<DiscordSocketClient>();
        _commands = services.GetRequiredService<InteractionService>();

        _client.Log += LogAsync;
        _commands.Log += LogAsync;

        _client.Ready += Ready;

        await _client.SetActivityAsync(BotConfig.Activity);

        await _client.LoginAsync(TokenType.Bot, BotConfig.Instance.Token);
        await _client.StartAsync();

        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        services.GetRequiredService<InteractionHandler>().InitializeAsync();
        services.GetRequiredService<CommandHandler>().Initialize();

        await Task.Delay(-1);
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<CommandHandler>()
            .BuildServiceProvider();
    }

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

    private async Task Ready()
    {
        await _commands.RegisterCommandsGloballyAsync();

        Log.Information($"Connected as {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}");
        Log.Information("Activity: {Type} {Name}", _client.Activity.Type, _client.Activity.Name);
    }
}