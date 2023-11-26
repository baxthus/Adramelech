using Adramelech.Configuration;
using Adramelech.Extensions;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class NekoCommand : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("neko", "Get a random neko image")]
    public async Task NekoAsync()
    {
        await DeferAsync();

        var response = await "https://nekos.life/api/v2/img/neko".Request<Neko>();
        if (response.IsDefault())
        {
            await Context.ErrorResponse("Error while fetching neko image", true);
            return;
        }

        await FollowupAsync(embed: new EmbedBuilder()
            .WithColor(BotConfig.EmbedColor)
            .WithImageUrl(response.Url)
            .WithFooter("Powered by nekos.life")
            .Build());
    }

    private struct Neko
    {
        public string Url { get; set; }
    }
}