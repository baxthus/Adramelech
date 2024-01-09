using Adramelech.Extensions;
using Adramelech.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Ping : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("ping", "Reply with Pong!")]
    public async Task PingAsync() =>
        await RespondAsync(
            embed: new EmbedBuilder()
                .WithColor(ConfigService.EmbedColor)
                .WithTitle("Pong!")
                .Build(),
            components: new ComponentBuilder()
                .WithButton("Velocity", "velocity")
                .WithButton("Author", style: ButtonStyle.Link, url: "https://abysmal.eu.org")
                .Build());
}

public class Velocity : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    [ComponentInteraction("velocity")]
    public async Task Button()
    {
        await Context.Interaction.UpdateAsync(p =>
        {
            p.Components = new ComponentBuilder()
                .WithButton("Velocity", "velocity", style: ButtonStyle.Success, disabled: true)
                .WithButton("Author", style: ButtonStyle.Link, url: "https://abysmal.eu.org")
                .Build();
        });

        await ReplyAsync(
            embed: new EmbedBuilder()
                .WithColor(ConfigService.EmbedColor)
                .WithTitle("__Adramelech Velocity Test__")
                .WithDescription($"Response time from our servers to Discord is {Context.Client.Latency}ms")
                .Build(),
            messageReference: Context.MessageReference());
    }
}