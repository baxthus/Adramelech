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
            await Context.ErrorResponse("Error sending DM");
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__DM Sent__")
            .WithDescription("Message sent successfully")
            .Build();
        
        await RespondAsync(embed: embed, ephemeral: true);
    }
}