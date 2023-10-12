using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class BanCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ban", "Ban a member")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [EnabledInDm(false)]
    public async Task Ban([Summary("user", "Mention a user")] SocketGuildUser member,
        [Summary("reason", "Reason for the ban")]
        string reason = "No reason provided",
        [Summary("prune_days", "Number of days to prune messages")] [MinValue(0)] [MaxValue(7)]
        int pruneDays = 0)
    {
        if (member.Id == Context.User.Id)
        {
            await Context.ErrorResponse("You cannot ban yourself");
            return;
        }

        if (member.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
        {
            await Context.ErrorResponse("You cannot ban a member with a higher or equal role");
            return;
        }

        await member.BanAsync(pruneDays, reason);

        await member.SendMessageAsync($"You've been banned from {Context.Guild.Name}. Reason: {reason}");

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Member Banned__")
            .WithDescription($"User {member.Username} has been banned")
            .AddField("Reason", $"`{reason}`")
            .AddField("Author", Context.User.Mention)
            .Build();

        await RespondAsync(embed: embed);
    }
}