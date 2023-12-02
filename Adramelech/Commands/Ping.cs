using System.Diagnostics;
using Adramelech.Configuration;
using Adramelech.Extensions;
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
                .WithColor(BotConfig.EmbedColor)
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
        Stopwatch timer = new();

        // This is a representation of the time it takes for the bot to send a request to Discord and get a response
        // Not the actual ping, as the bot uses a websocket connection to Discord
        // Too bad
        timer.Start();
        await new HttpClient().GetAsync("https://discord.com/api/v9");
        timer.Stop();

        await Context.Interaction.UpdateAsync(p =>
        {
            p.Components = new ComponentBuilder()
                .WithButton("Velocity", "velocity", style: ButtonStyle.Success, disabled: true)
                .WithButton("Author", style: ButtonStyle.Link, url: "https://abysmal.eu.org")
                .Build();
        });

        await ReplyAsync(
            embed: new EmbedBuilder()
                .WithColor(BotConfig.EmbedColor)
                .WithTitle("__Adramelech Velocity Test__")
                .WithDescription($"Response time from our servers to Discord is {timer.ElapsedMilliseconds}ms")
                .Build(),
            messageReference: Context.MessageReference());
    }
}