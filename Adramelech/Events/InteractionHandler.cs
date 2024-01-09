using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Events;

public class InteractionHandler(
    DiscordSocketClient client,
    InteractionService interactionService,
    IServiceProvider services)
{
    public void Initialize() => client.InteractionCreated += HandleInteraction;

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var ctx = CreateGenetic(interaction, client);
            await interactionService.ExecuteCommandAsync(ctx, services);
        }
        catch (Exception ex)
        {
            Log.Error("Error while handling interaction: {Message}", ex.Message);
            // This is a hacky way to delete the original response if an error occurs
            // Don't work btw
            await interaction.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }


    private static IInteractionContext CreateGenetic(SocketInteraction interaction, DiscordSocketClient client) =>
        interaction switch
        {
            SocketModal modal => new SocketInteractionContext<SocketModal>(client, modal),
            SocketUserCommand user => new SocketInteractionContext<SocketUserCommand>(client, user),
            SocketSlashCommand slash => new SocketInteractionContext<SocketSlashCommand>(client, slash),
            SocketMessageCommand message => new SocketInteractionContext<SocketMessageCommand>(client, message),
            SocketMessageComponent component => new SocketInteractionContext<SocketMessageComponent>(client, component),
            _ => throw new InvalidOperationException("This interaction type is not supported.")
        };
}