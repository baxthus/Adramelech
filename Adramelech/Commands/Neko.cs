using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class NekoCommand : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("neko", "Get a random neko image")]
    public async Task NekoAsync()
    {
        var response = await Utilities.Request<Neko>("https://nekos.life/api/v2/img/neko");
        if (response.IsInvalid())
        {
            await Context.ErrorResponse("Error while fetching neko image");
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithImageUrl(response.Url)
            .WithFooter("Powered by nekos.life")
            .Build();

        await RespondAsync(embed: embed);
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private struct Neko
    {
        public string Url { get; set; }
    }
}