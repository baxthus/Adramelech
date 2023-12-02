using Discord;
using Discord.Interactions;
using Serilog;

namespace Adramelech.Services;

public class CommandHandler
{
    private readonly InteractionService _interactionService;

    public CommandHandler(InteractionService interactionService) => _interactionService = interactionService;

    public void Initialize() => _interactionService.SlashCommandExecuted += SlashCommandExecuted;

    private static Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext interaction,
        IResult result)
    {
        if (result.IsSuccess) return Task.CompletedTask;

        Log.Error("Error while executing slash command: {ErrorReason}", result.ErrorReason);
        Log.Error(result.ErrorReason);

        return Task.CompletedTask;
    }
}