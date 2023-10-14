using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;

namespace Adramelech.Commands;

public class Cat : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("cat", "Get a random cat image")]
    public async Task CatAsync()
    {
        var response = await Utilities.Request<CatResponse>("https://cataas.com/cat?json=true");
        if (response.IsInvalid())
        {
            await Context.ErrorResponse("Failed to get cat image");
            return;
        }
        
        if (response.Owner is null or "null" or "Null" or "")
            response.Owner = "Unknown";

        var embed = new EmbedBuilder()
            .WithColor(Config.Bot.EmbedColor)
            .WithImageUrl("https://cataas.com" + response.Url)
            .WithFooter($"Owner: {response.Owner}\nPowered by cataas.com")
            .Build();

        await RespondAsync(embed: embed);
    }
    
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private struct CatResponse
    {
        public string Owner { get; set; }
        public string Url { get; set; }
    }
}