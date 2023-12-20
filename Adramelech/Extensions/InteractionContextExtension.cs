using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Extensions;

/// <summary>
/// Extension class for <see cref="IInteractionContext"/>
/// </summary>
public static class InteractionContextExtension
{
    /// <summary>
    /// Respond with a ephemeral error message
    /// </summary>
    /// <param name="context">The interaction context (can be implicit)</param>
    /// <param name="description">The description of the error (optional)</param>
    /// <param name="origin">The origin of the interaction; default is <see cref="InteractionOrigin.SlashCommand"/></param>
    /// <param name="toDm">True if the error should be sent to the user's DM; default is false</param>
    private static async Task SendError(this IInteractionContext context, string? description = null,
        InteractionOrigin origin = InteractionOrigin.SlashCommand, bool toDm = false)
    {
        var embed = description is null
            ? new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("**Error!**")
                .Build()
            : new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("**Error!**")
                .WithDescription(description)
                .Build();

        if (toDm)
        {
            await context.User.SendMessageAsync(embed: embed);
            return;
        }

        switch (origin)
        {
            case InteractionOrigin.SlashCommand:
                await context.Interaction.RespondAsync(embed: embed, ephemeral: true);
                break;
            case InteractionOrigin.SlashCommandDeferred:
                // TODO: Find a better way to send error message when deferred
                // NOTE: Every command that uses a external API should be deferred.
                await context.Interaction.FollowupAsync("opps...")
                    .ContinueWith(async x => await x.Result.DeleteAsync());
                await context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
                break;
            case InteractionOrigin.Component:
                var componentContext = context as SocketInteractionContext<SocketMessageComponent>;
                await componentContext!.Interaction.UpdateAsync(p =>
                {
                    p.Embed = embed;
                    // Remove the buttons and content
                    p.Content = "";
                    p.Components = new ComponentBuilder().Build();
                });
                break;
            default:
                // If this happens, something is very wrong, consider tracking what interaction caused this
                Log.Warning(
                    "Unknown interaction origin in interaction {InteractionId} from {Username} ({UserId}) at {GuildName} ({GuildId})",
                    context.Interaction.Id, context.User.Username, context.User.Id, context.Guild.Name,
                    context.Guild.Id);
                break;
        }
    }

    /// <summary>
    /// Respond with a ephemeral error message
    /// </summary>
    /// <param name="context">The interaction context (can be implicit)</param>
    /// <param name="description">The description of the error (optional)</param>
    /// <param name="isDeferred">True if the interaction is deferred; default is false</param>
    /// <param name="toDm">True if the error should be sent to the user's DM; default is false</param>
    /// <remarks>This is a overload for <see cref="SendError(IInteractionContext,string?,InteractionOrigin,bool)"/></remarks>
    public static Task SendError(this SocketInteractionContext<SocketSlashCommand> context,
        string? description = null, bool isDeferred = false, bool toDm = false) =>
        SendError(context, description,
            isDeferred ? InteractionOrigin.SlashCommandDeferred : InteractionOrigin.SlashCommand, toDm);

    /// <summary>
    /// Respond with a ephemeral error message
    /// </summary>
    /// <param name="context">The interaction context (can be implicit)</param>
    /// <param name="description">The description of the error (optional)</param>
    /// <param name="toDm">True if the error should be sent to the user's DM; default is false</param>
    /// <remarks>This is a overload for <see cref="SendError(IInteractionContext,string?,InteractionOrigin,bool)"/></remarks>
    public static Task SendError(this SocketInteractionContext<SocketMessageComponent> context,
        string? description = null, bool toDm = false) =>
        SendError(context, description, InteractionOrigin.Component, toDm);

    /// <summary>
    /// Get the <see cref="MessageReference"/> from a <see cref="IInteractionContext"/>
    /// </summary>
    /// <param name="context">The interaction context (can be implicit)</param>
    /// <returns>The <see cref="MessageReference"/></returns>
    /// <remarks>Currently only supports <see cref="SocketInteractionContext{SocketMessageComponent}"/></remarks>
    public static MessageReference? MessageReference(this IInteractionContext context) =>
        context switch
        {
            SocketInteractionContext<SocketMessageComponent> componentContext => new MessageReference(
                componentContext.Interaction.Message.Id,
                componentContext.Interaction.Channel.Id,
                componentContext.Interaction.GuildId),
            _ => null
        };
}

public enum InteractionOrigin
{
    SlashCommand,
    SlashCommandDeferred,
    Component
}