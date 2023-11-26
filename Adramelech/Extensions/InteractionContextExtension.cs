using Discord;

namespace Adramelech.Extensions;

/// <summary>
/// Extension class for <see cref="IInteractionContext"/>
/// </summary>
public static class InteractionContextExtension
{
    /// <summary>
    /// Respond with a ephemeral error message
    /// </summary>
    /// <param name="ctx">The interaction context (can be implicit)</param>
    /// <param name="desc">The description of the error (optional)</param>
    /// <param name="isDeferred">Whether the response is deferred or not; default is false</param>
    /// <remarks>This is just to make the code look cleaner</remarks>
    public static async Task ErrorResponse(this IInteractionContext ctx, string? desc = null, bool isDeferred = false)
    {
        var embed = desc == null
            ? new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("**Error!**")
            : new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("**Error!**")
                .WithDescription(desc);

        if (isDeferred)
        {
            // Send a dummy message first so I can make the second message ephemeral
            // Because if I defer without making ephemeral, the followup message will be public
            // This has the drawback of the replying indicator showing the original message as deleted
            // Someday I'll find a better way to do this
            // NOTE: Every command that uses a external API will be deferred
            var msg = await ctx.Interaction.FollowupAsync("opps...");
            await msg.DeleteAsync();
            await ctx.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: true);
        }
        else
            await ctx.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}