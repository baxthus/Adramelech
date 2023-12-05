using Adramelech.Configuration;
using Adramelech.Extensions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class SendDm : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("send-dm", "Send a DM to a user")]
    [RequireOwner]
    public async Task SendDmAsync([Summary("user", "The user to send the DM to")] SocketUser user,
        [Summary("message", "The message to send")]
        string message)
    {
        try
        {
            await user.SendMessageAsync(message);
        }
        catch (Exception)
        {
            await Context.SendError("Error sending DM");
            return;
        }

        await RespondAsync(
            embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithTitle("__DM Sent__")
                .WithDescription("Message sent successfully")
                .Build(),
            ephemeral: true);
    }
}