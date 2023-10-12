using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;

namespace Adramelech.Commands;

public class ClearCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("clear", "Clears the chat")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    [RequireContext(ContextType.Guild)]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task Clear([Summary("amount", "Amount of messages")] [MinValue(1)] [MaxValue(100)] int amount)
    {
        var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();
        if (!messages.Any())
        {
            await Context.ErrorResponse("No messages to delete");
            return;
        }

        try
        {
            await Context.Guild.GetTextChannel(Context.Channel.Id).DeleteMessagesAsync(messages);
        }
        catch
        {
            await Context.ErrorResponse("Messages are older than 14 days");
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Clear__")
            .WithDescription($"Successfully deleted {messages.Count()} messages\n" +
                             $"Command executed by {Context.User.Mention}")
            .Build();

        await RespondAsync(embed: embed);
        
        await Task.Delay(5000);

        await DeleteOriginalResponseAsync();
    }
}