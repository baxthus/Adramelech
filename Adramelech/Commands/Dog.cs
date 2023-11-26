using Adramelech.Configuration;
using Adramelech.Extensions;
using Adramelech.Utilities;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Dog : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("dog", "Gets a random dog image")]
    public async Task DogAsync()
    {
        await DeferAsync();

        var response = await "https://dog.ceo/api/breeds/image/random".Request<DogResponse>();
        if (response.IsDefault() || response.Status != "success")
        {
            await Context.ErrorResponse("Failed to get dog image", true);
            return;
        }

        await FollowupAsync(embed: new EmbedBuilder()
            .WithColor(BotConfig.EmbedColor)
            .WithImageUrl(response.Message)
            .WithFooter("Powered by dog.ceo")
            .Build());
    }

    private struct DogResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}