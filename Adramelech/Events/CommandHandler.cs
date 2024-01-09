using Discord;
using Discord.Interactions;
using Serilog;

namespace Adramelech.Events;

public class CommandHandler(InteractionService interactionService)
{
    public void Initialize() => interactionService.SlashCommandExecuted += SlashCommandExecuted;

    private static Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext interaction,
        IResult result)
    {
        if (result.IsSuccess) return Task.CompletedTask;

        Log.Error("Error while executing slash command: {ErrorReason}", result.ErrorReason);
        Log.Error(result.ErrorReason);

        return Task.CompletedTask;
    }
}