using System.Diagnostics.CodeAnalysis;
using Adramelech.Configuration;
using Adramelech.Extensions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Clear : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("clear", "Clears the chat")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    [EnabledInDm(false)]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task ClearAsync([Summary("amount", "Amount of messages")] [MinValue(1)] [MaxValue(100)] int amount)
    {
        var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();
        if (!messages.Any())
        {
            await Context.SendError("No messages to delete");
            return;
        }

        try
        {
            await Context.Guild.GetTextChannel(Context.Channel.Id).DeleteMessagesAsync(messages);
        }
        catch
        {
            await Context.SendError("Messages are older than 14 days");
            return;
        }

        await RespondAsync(embed: new EmbedBuilder()
            .WithColor(BotConfig.EmbedColor)
            .WithTitle("__Clear__")
            .WithDescription($"Successfully deleted {messages.Count()} messages\n" +
                             $"Command executed by {Context.User.Mention}")
            .Build());

        await Task.Delay(5000);

        try
        {
            await DeleteOriginalResponseAsync();
        }
        catch
        {
            // ignored
        }
    }
}