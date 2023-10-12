using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Services;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;

    public CommandHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
    {
        _client = client;
        _commands = commands;
        _services = services;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.InteractionCreated += HandleInteraction;

        _commands.SlashCommandExecuted += SlashCommandExecuted;
        // _commands.ContextCommandExecuted += ContextCommandExecuted;
    }

    // private static Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    // {
    //     if (!arg3.IsSuccess)
    //     {
    //         Log.Error(arg3.ErrorReason);
    //     }
    //
    //     return Task.CompletedTask;
    // }

    private static Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            Log.Error(arg3.ErrorReason);
        }

        return Task.CompletedTask;
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        if (arg.Type != InteractionType.ApplicationCommand)
            return;
        
        try
        {
            var ctx = new SocketInteractionContext(_client, arg);
            await _commands.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing command");
            await arg.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }
}