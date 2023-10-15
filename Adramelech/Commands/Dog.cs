using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Dog : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("dog", "Gets a random dog image")]
    public async Task DogAsync()
    {
        var response = await Utilities.Request<DogResponse>("https://dog.ceo/api/breeds/image/random");
        if (response.IsInvalid() || response.Status != "success")
        {
            await Context.ErrorResponse("Failed to get dog image");
            return;
        }
        
        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithImageUrl(response.Message)
            .WithFooter("Powered by dog.ceo")
            .Build();
        
        await RespondAsync(embed: embed);
    }
    
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private struct DogResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}