using Discord;
using Discord.Interactions;

namespace Adramelech.Commands;

public class Echo : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("echo", "Echoes the given text")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public async Task EchoAsync([Summary("text", "The text to echo")] string text)
    {
        var final = text + $"\n\n \\- {Context.User.Mention}";
        
        await Context.Channel.SendMessageAsync(final);

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("Message sent")
            .Build();

        await RespondAsync(embed: embed, ephemeral: true);
    }
}