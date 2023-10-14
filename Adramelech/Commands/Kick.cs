﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Kick : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("kick", "Kick a member")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    [EnabledInDm(false)]
    public async Task KickAsync([Summary("user", "Mention a user")] SocketGuildUser member,
        [Summary("reason", "Reason for the kick")]
        string reason = "No reason provided",
        [Summary("prune_days", "Number of days to prune messages")] [MinValue(0)] [MaxValue(7)]
        int pruneDays = 0)
    {
        if (member.Id == Context.User.Id)
        {
            await Context.ErrorResponse("You cannot kick yourself");
            return;
        }
        
        if (member.Hierarchy >= Context.Guild.CurrentUser.Hierarchy)
        {
            await Context.ErrorResponse("You cannot kick a member with a higher or equal role");
            return;
        }

        await member.BanAsync(pruneDays, reason);
        
        await member.SendMessageAsync($"You've been kicked from {Context.Guild.Name}. Reason: {reason}");
        
        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Member Kicked__")
            .WithDescription($"User {member.Username} has been kicked")
            .AddField("Reason", $"`{reason}`")
            .AddField("Author", Context.User.Mention)
            .Build();
        
        await RespondAsync(embed: embed);
    }
}