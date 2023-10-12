using Discord.WebSocket;
using Serilog;

namespace Adramelech.Services;

public class ModalHandler
{
    private readonly DiscordSocketClient _client;

    public ModalHandler(DiscordSocketClient client) => _client = client;

    public Task InitializeAsync()
    {
        _client.ModalSubmitted += HandleModal;
        return Task.CompletedTask;
    }

    private async Task HandleModal(SocketModal modal)
    {
        var modals = typeof(Modal).Assembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(Modal)))
            .Select(x => (Modal)Activator.CreateInstance(x)!)
            .ToArray();

        var selected = modals.FirstOrDefault(x => x.Id == modal.Data.CustomId);
        if (selected == null)
        {
            await modal.RespondAsync("This modal doesn't exist.", ephemeral: true);
            return;
        }

        try
        {
            await selected.Execute(_client, modal);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing modal {ModalId}", selected.Id);
        }
    }
}

public abstract class Modal
{
    public abstract string Id { get; }

    public abstract Task Execute(DiscordSocketClient client, SocketModal modal);
}