using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Services;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _services;
    
    public InteractionHandler(DiscordSocketClient client, InteractionService interactionService, IServiceProvider services)
    {
        _client = client;
        _interactionService = interactionService;
        _services = services;
    }

    public Task InitializeAsync()
    {
        _client.InteractionCreated += HandleInteraction;
        return Task.CompletedTask;
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var ctx = CreateGenetic(interaction, _client);
            await _interactionService.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            Log.Error("Error while handling interaction: {Message}", ex.Message);
            // This is a hacky way to delete the original response if an error occurs
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