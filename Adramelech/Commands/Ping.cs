﻿using Discord;
using Discord.Interactions;

namespace Adramelech.Commands;

public class PingCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Reply with Pong!")]
    public async Task Ping()
    {
        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithTitle("Pong!")
            .Build();

        var buttons = new ComponentBuilder()
            .WithButton("Velocity", "SpeedTest")
            .WithButton("Author", style: ButtonStyle.Link, url: "https://abysmal.eu.org")
            .Build();
        
        await RespondAsync(embed: embed, components: buttons) ;
    }
}