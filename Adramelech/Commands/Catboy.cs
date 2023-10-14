// The catboys api is dead, so this command is dead too.
// I thought the waifu.pics api had catboys, but it doesn't.
// I will keep this file here for future reference.

// using System.Diagnostics.CodeAnalysis;
// using Discord;
// using Discord.Interactions;
//
// namespace Adramelech.Commands;
//
// public class Catboy : InteractionModuleBase<SocketInteractionContext>
// {
//     [SlashCommand("catboy", "Sends a random catboy image")]
//     public async Task CatboyAsync()
//     {
//         var response = await Utilities.Request<CatboyResponse>("https://api.waifu.pics/sfw/neko");
//         if (response.IsInvalid())
//         {
//             await Context.ErrorResponse("Something went wrong while fetching the image.");
//             return;
//         }
//
//         var embed = new EmbedBuilder()
//             .WithColor(Config.Bot.EmbedColor)
//             .WithImageUrl(response.Url)
//             .WithFooter($"Powered by waifu.pics")
//             .Build();
//         
//         await RespondAsync(embed: embed);
//     }
//
//     [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
//     private struct CatboyResponse
//     {
//         public string Url { get; set; }
//     }
// }