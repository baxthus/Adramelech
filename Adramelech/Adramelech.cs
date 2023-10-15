using System.Reflection;
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

        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        
        await services.GetRequiredService<InteractionHandler>().InitializeAsync();
        await services.GetRequiredService<CommandHandler>().InitializeAsync();
        
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