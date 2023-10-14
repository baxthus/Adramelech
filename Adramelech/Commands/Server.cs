using Discord;
using Discord.Interactions;

namespace Adramelech.Commands;

public class Server : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("server", "Get server info")]
    [RequireContext(ContextType.Guild)]
    public async Task ServerAsync()
    {
        var guildOwner = Context.Guild.Owner;
        var createdAt = Context.Guild.CreatedAt.ToUnixTimeSeconds();
        
        var totalMembers = Context.Guild.MemberCount;
        
        var premiumTier = Context.Guild.PremiumTier != PremiumTier.None
            ? $" (Level {Context.Guild.PremiumTier})"
            : string.Empty;

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
            .AddField("Server owner", $"{guildOwner.Username} (`{guildOwner.Id}`)", true)
            .AddField("Server ID", Context.Guild.Id, true)
            .AddField("Members", totalMembers, true)
            .AddField("Roles", Context.Guild.Roles.Count, true)
            .AddField("Channels", Context.Guild.Channels.Count, true)
            .AddField("Server Boosts", $"{Context.Guild.PremiumSubscriptionCount} {premiumTier}", true)
            .AddField("Created at", $"<t:{createdAt}:f> (<t:{createdAt}:R>)", true)
            .Build();

        await RespondAsync(embed: embed);
    }
}