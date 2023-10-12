using Discord.WebSocket;
using Serilog;

namespace Adramelech.Services;

public class ButtonHandler
{
    private readonly DiscordSocketClient _client;
    
    public ButtonHandler(DiscordSocketClient client) => _client = client;
    
    public Task InitializeAsync()
    {
        _client.ButtonExecuted += HandleButton;
        return Task.CompletedTask;
    }

    private async Task HandleButton(SocketMessageComponent component)
    {
        var buttons = typeof(Button).Assembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(Button)))
            .Select(x => (Button) Activator.CreateInstance(x)!)
            .ToArray();
        
        var button = buttons.FirstOrDefault(x => x.Id == component.Data.CustomId);
        if (button == null)
        {
            await component.RespondAsync("This button doesn't exist.", ephemeral: true);
            return;
        }

        try
        {
            await button.Execute(_client, component);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing button {ButtonId}", button.Id);
        }
    }
}

public abstract class Button
{
    public abstract string Id { get; }

    public abstract Task Execute(DiscordSocketClient client, SocketMessageComponent component);
}
