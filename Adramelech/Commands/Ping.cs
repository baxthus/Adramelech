using System.Diagnostics;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Ping : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("ping", "Reply with Pong!")]
    public async Task PingAsync()
    {
        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("Pong!")
            .Build();

        var buttons = new ComponentBuilder()
            .WithButton("Velocity", "velocity")
            .WithButton("Author", style: ButtonStyle.Link, url: "https://abysmal.eu.org")
            .Build();
        
        await RespondAsync(embed: embed, components: buttons) ;
    }
}

public class Velocity : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    [ComponentInteraction("velocity")]
    public async Task Button()
    {
        Stopwatch timer = new();
            
        timer.Start();
        await new HttpClient().GetAsync("https://discord.com/api/v9");
        timer.Stop();
            
        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("__Adramelech Velocity Test__")
            .WithDescription($"Response time from our servers to Discord is {timer.ElapsedMilliseconds}ms")
            .Build();

        await Context.Interaction.UpdateAsync(p =>
        {
            p.Embed = embed;
            // This will remove the buttons from the message
            p.Components = new ComponentBuilder().Build();
        });
    }
}