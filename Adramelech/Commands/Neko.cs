using Discord;
using Discord.Interactions;

namespace Adramelech.Commands;

public class NekoCommand : InteractionModuleBase<SocketInteractionContext>
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

    private struct Neko
    {
        public string Url { get; set; }
    }
}