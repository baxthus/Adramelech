using Adramelech.Services;
using Discord.Interactions;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Feedback : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("feedback", "Send feedback to the developers")]
    public async Task FeedbackAsync() => await RespondWithModalAsync<FeedbackModal>("feedback_modal");

    public class FeedbackModal : IModal
    {
        public string Title => "Feedback";

        [InputLabel("Message")]
        [ModalTextInput("message", TextInputStyle.Paragraph, "Please enter your feedback here")]
        public required string Message { get; set; }
    }
}

public class FeedbackModalResponse(ConfigService configService)
    : InteractionModuleBase<SocketInteractionContext<SocketModal>>
{
    [ModalInteraction(("feedback_modal"))]
    public async Task Modal(Feedback.FeedbackModal modal)
    {
        var message = modal.Message;

        var webhook = new DiscordWebhookClient(configService.FeedbackWebhook);

        await webhook.SendMessageAsync(
            username: "Adramelech Feedback",
            avatarUrl: Context.Client.CurrentUser.GetAvatarUrl(size: 4096),
            embeds: new[]
            {
                new EmbedBuilder()
                    .WithColor(ConfigService.EmbedColor)
                    .WithTitle("__Adramelech Feedback__")
                    .WithDescription($"From `{Context.User.Username}` (`{Context.User.Id}`)")
                    .WithThumbnailUrl(Context.User.GetAvatarUrl(size: 4096))
                    .AddField("Message", $"```{message}```")
                    .Build()
            });

        await RespondAsync("Your feedback has been sent to the developers.", ephemeral: true);
    }
}