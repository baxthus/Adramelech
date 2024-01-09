using Adramelech.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Server : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("server", "Get server info")]
    [RequireContext(ContextType.Guild)]
    public async Task ServerAsync()
    {
        var guildOwner = Context.Guild.Owner;
        var createdAt = Context.Guild.CreatedAt.ToUnixTimeSeconds();

        var premiumTier = Context.Guild.PremiumTier == PremiumTier.None
            ? string.Empty
            : $" (Level {Context.Guild.PremiumTier})";

        await RespondAsync(embed: new EmbedBuilder()
            .WithColor(ConfigService.EmbedColor)
            .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
            .AddField("Server owner", $"{guildOwner.Username} (`{guildOwner.Id}`)", true)
            .AddField("Server ID", Context.Guild.Id, true)
            .AddField("Members", Context.Guild.MemberCount, true)
            .AddField("Roles", Context.Guild.Roles.Count, true)
            .AddField("Channels", Context.Guild.Channels.Count, true)
            .AddField("Server Boosts", $"{Context.Guild.PremiumSubscriptionCount}{premiumTier}", true)
            .AddField("Created at", $"<t:{createdAt}:f> (<t:{createdAt}:R>)", true)
            .Build());
    }
}