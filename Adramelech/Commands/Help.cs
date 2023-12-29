using System.Text;
using Adramelech.Tools;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Help : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("help", "Get help with the bot")]
    public async Task HelpAsync(
        [Summary("separate-rows", "Whether to separate rows with lines")] bool separateRows = false)
    {
        var commands = await Context.Client.GetGlobalApplicationCommandsAsync();

        var content = new UnicodeSheet(separateRows)
            .AddColumn("Command", commands.Select(x => x.Name))
            .AddColumn("Description", commands.Select(x => x.Description))
            .Build();

        var info = await Context.Client.GetApplicationInfoAsync();

        await RespondWithFileAsync(
            text: "> ## Help\n" +
                  "### :information: Information\n" +
                  $"Bot created by {info.Owner.Mention}\n" +
                  "[Website](<https://abysmal.eu.org>) | [Github](<https://github.com/baxthus>) | [Donation](<https://dub.sh/BaxthusDonation>)\n" +
                  "### :page_facing_up: Commands",
            fileName: "commands.diff",
            fileStream: new MemoryStream(Encoding.UTF8.GetBytes(content)),
            ephemeral: true);
    }
}