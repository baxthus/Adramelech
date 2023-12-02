﻿using Discord;
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
    public static async Task ErrorResponse(this IInteractionContext context, string? description = null,
        InteractionOrigin origin = InteractionOrigin.SlashCommand)
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

        switch (origin)
        {
            case InteractionOrigin.SlashCommand:
                await context.Interaction.RespondAsync(embed: embed, ephemeral: true);
                break;
            case InteractionOrigin.SlashCommandDeferred:
                // ATTENTION: This is outrageous, kill that with fire
                // Send a dummy message first so the second message can be ephemeral
                // Because if I defer without making ephemeral, the followup message will be public
                // This has the drawback of the replying indicator showing the original message as deleted
                // Someday I'll find a better way to do this
                // NOTE: Every command that uses a external API should be deferred
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
    /// <remarks>This is a overload for <see cref="ErrorResponse(IInteractionContext,string?,InteractionOrigin)"/> to make the code look cleaner</remarks>
    public static async Task ErrorResponse(this SocketInteractionContext<SocketSlashCommand> context,
        string? description = null, bool isDeferred = false) =>
        await ErrorResponse(context, description,
            isDeferred ? InteractionOrigin.SlashCommandDeferred : InteractionOrigin.SlashCommand);

    /// <summary>
    /// Get the <see cref="MessageReference"/> from a <see cref="IInteractionContext"/>
    /// </summary>
    /// <param name="context">The interaction context (can be implicit)</param>
    /// <returns>The <see cref="MessageReference"/></returns>
    /// <remarks>Currently only supports <see cref="SocketInteractionContext{SocketMessageComponent}"/></remarks>
    // This is the proof that God abandoned us
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