using Adramelech.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Adramelech;

public class Adramelech
{
    public static Task Main() => new Adramelech().MainAsync();

    private DiscordSocketClient _client;
    private InteractionService _commands;

    private async Task MainAsync()
    {
        Database.CreateConnection();
        Config.Verify();
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        
        var services = ConfigureServices();

        _client = services.GetRequiredService<DiscordSocketClient>();
        _commands = services.GetRequiredService<InteractionService>();
        
        _client.Log += LogAsync;
        _commands.Log += LogAsync;
        
        _client.Ready += Ready;

        await _client.SetActivityAsync(Config.Bot.Activity);

        await _client.LoginAsync(TokenType.Bot, Config.Bot.Token);
        await _client.StartAsync();
        
        await services.GetRequiredService<CommandHandler>().InitializeAsync();
        await services.GetRequiredService<ButtonHandler>().InitializeAsync();
        await services.GetRequiredService<ModalHandler>().InitializeAsync();
        
        await Task.Delay(-1);
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandHandler>()
            .AddSingleton<ButtonHandler>()
            .AddSingleton<ModalHandler>()
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
        await _commands.RegisterCommandsGloballyAsync(true);
        
        Log.Information($"Connected as {_client.CurrentUser}");
        Log.Information("Activity: {Type} {Name}", _client.Activity.Type, _client.Activity.Name);
    }
}