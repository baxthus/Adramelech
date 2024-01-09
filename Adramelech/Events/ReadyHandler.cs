using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Events;

public class ReadyHandler(DiscordSocketClient client, InteractionService interactionService)
{
    public void Initialize() => client.Ready += Ready;

    private async Task Ready()
    {
        await interactionService.RegisterCommandsGloballyAsync();

        Log.Information("Connected to Discord as {Username}#{Discriminator}", client.CurrentUser.Username,
            client.CurrentUser.Discriminator);
        Log.Information("Activity: {Type} {Name}", client.Activity.Type, client.Activity.Name);
    }
}