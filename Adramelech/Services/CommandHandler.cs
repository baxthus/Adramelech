using Discord;
using Discord.Interactions;
using Serilog;

namespace Adramelech.Services;

public class CommandHandler
{
    private readonly InteractionService _commands;

    public CommandHandler(InteractionService commands) => _commands = commands;

    public void Initialize() => _commands.SlashCommandExecuted += SlashCommandExecuted;

    private static Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        if (arg3.IsSuccess) return Task.CompletedTask;

        Log.Error("Error while executing slash command: {ErrorReason}", arg3.ErrorReason);
        Log.Error(arg3.ErrorReason);

        return Task.CompletedTask;
    }
}