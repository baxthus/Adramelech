using Discord;
using Discord.Interactions;
using Serilog;

namespace Adramelech.Services;

public class CommandHandler
{
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;

    public CommandHandler(InteractionService commands, IServiceProvider services)
    {
        _commands = commands;
        _services = services;
    }

    public Task InitializeAsync()
    {
        _commands.SlashCommandExecuted += SlashCommandExecuted;
        return Task.CompletedTask;
    }

    private static Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            Log.Error("Error while executing slash command: {ErrorReason}", arg3.ErrorReason);
            Log.Error(arg3.ErrorReason);
        }

        return Task.CompletedTask;
    }
}