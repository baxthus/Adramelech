using Adramelech.Configuration;
using Adramelech.Extensions;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Cat : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("cat", "Get a random cat image")]
    public async Task CatAsync()
    {
        await DeferAsync();

        var response = await "https://api.thecatapi.com/v1/images/search".Request<CatResponse[]>();
        if (response.IsDefault())
        {
            await Context.ErrorResponse("Failed to get cat image", true);
            return;
        }

        await FollowupAsync(embed: new EmbedBuilder()
            .WithColor(BotConfig.EmbedColor)
            .WithImageUrl(response!.First().Url)
            .WithFooter($"Powered by thecatapi.com")
            .Build());
    }

    private struct CatResponse
    {
        public string Url { get; set; }
    }
}