using Adramelech.Configuration;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Echo : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("echo", "Echoes the given text")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public async Task EchoAsync() => await RespondWithModalAsync<EchoModal>("echo_modal");

    public class EchoModal : IModal
    {
        public string Title => "Echo";

        [InputLabel("Message")]
        [ModalTextInput("message", TextInputStyle.Paragraph, "Please enter your message here")]
        public required string Message { get; set; }
    }
}

public class EchoModalResponse : InteractionModuleBase<SocketInteractionContext<SocketModal>>
{
    [ModalInteraction("echo_modal")]
    public async Task ModalAsync(Echo.EchoModal modal)
    {
        var message = modal.Message;

        var final = $"{message}\n\n \\- {Context.User.Mention}";

        await ReplyAsync(final);

        await RespondAsync(
            embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithTitle("Message sent")
                .Build(),
            ephemeral: true);
    }
}