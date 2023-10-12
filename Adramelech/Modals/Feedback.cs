using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Modal = Adramelech.Services.Modal;

namespace Adramelech.Modals;

public class Feedback : Modal
{
    public override string Id => "Feedback";

    public override async Task Execute(DiscordSocketClient client, SocketModal modal)
    {
        var message = modal.Data.Components.FirstOrDefault(x => x.CustomId == "Message")?.Value!;

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Adramelech Feedback__")
            .WithDescription($"From `{modal.User.Username}` (`{modal.User.Id}`)")
            .AddField("Message", $"```{message}```")
            .Build();

        var webhook = new DiscordWebhookClient(Config.Bot.FeedbackWebhook);

        await webhook.SendMessageAsync(embeds: new[] { embed }, username: "Adramelech Feedback", avatarUrl: Config.Bot.Avatar);
        
        await modal.RespondAsync("Your feedback has been sent to the developers.", ephemeral: true);
    }
}