using Discord.Interactions;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Feedback : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("feedback", "Send feedback to the developers")]
    public async Task FeedbackAsync() => await RespondWithModalAsync<FeedbackModal>("feedback_modal");

    // ReSharper disable once ClassNeverInstantiated.Global
    public class FeedbackModal : IModal
    {
        public string Title => "Feedback";

        [InputLabel("Message")]
        [ModalTextInput("message", TextInputStyle.Paragraph, "Please enter your feedback here")]
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string Message { get; set; } = null!;
    }
}

public class FeedbackModalResponse : InteractionModuleBase<SocketInteractionContext<SocketModal>>
{
    [ModalInteraction(("feedback_modal"))]
    public async Task Modal(Feedback.FeedbackModal modal)
    {
        var message = modal.Message;

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Adramelech Feedback__")
            .WithDescription($"From `{Context.User.Username}` (`{Context.User.Id}`)")
            .WithThumbnailUrl(Context.User.GetAvatarUrl(size: 4096))
            .AddField("Message", $"```{message}```")
            .Build();

        var webhook = new DiscordWebhookClient(Config.Bot.FeedbackWebhook);

        await webhook.SendMessageAsync(embeds: new[] { embed }, username: "Adramelech Feedback",
            avatarUrl: Context.Client.CurrentUser.GetAvatarUrl(size: 4096));

        await RespondAsync("Your feedback has been sent to the developers.", ephemeral: true);
    }
}