using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Services;

public class ReadyHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;

    public ReadyHandler(DiscordSocketClient client, InteractionService interactionService)
    {
        _client = client;
        _interactionService = interactionService;
    }

    public void Initialize() => _client.Ready += Ready;

    private async Task Ready()
    {
        await _interactionService.RegisterCommandsGloballyAsync();

        Log.Information("Connected to Discord as {Username}#{Discriminator}", _client.CurrentUser.Username,
            _client.CurrentUser.Discriminator);
        Log.Information("Activity: {Type} {Name}", _client.Activity.Type, _client.Activity.Name);
    }
}