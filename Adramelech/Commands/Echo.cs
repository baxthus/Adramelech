using Adramelech.Configuration;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Echo : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("echo", "Echoes the given text")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public async Task EchoAsync([Summary("text", "The text to echo")] string text)
    {
        // The "\\-" is to the character "-" not be interpreted as a markdown list
        // Is necessary two "\\", because the first is to escape the second, and the second is to escape the "-"
        // I love string interpolation
        var final = $"{text}\n\n \\- {Context.User.Mention}";

        await Context.Channel.SendMessageAsync(final);

        await RespondAsync(
            embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithTitle("Message sent")
                .Build(),
            ephemeral: true);
    }
}