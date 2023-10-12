using System.Diagnostics;
using Adramelech.Services;
using Discord;
using Discord.WebSocket;

namespace Adramelech.Buttons;

public class SpeedTest : Button
{
    public override string Id => "SpeedTest";

    public override async Task Execute(DiscordSocketClient client, SocketMessageComponent component)
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

        await component.RespondAsync(embed: embed);
    }
}