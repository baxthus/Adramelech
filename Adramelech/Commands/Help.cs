using System.Text;
using Adramelech.Utilities;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Help : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("help", "Get help with the bot")]
    public async Task HelpAsync()
    {
        var commands = await Context.Client.GetGlobalApplicationCommandsAsync();

        StringBuilder content = new();

        // content.AppendLine("""
        //                    ---      _____                                          _       _      _     _      ----
        //                    ---     / ____|                                        | |     | |    (_)   | |     ----
        //                    ---    | |     ___  _ __ ___  _ __ ___   __ _ _ __   __| |___  | |     _ ___| |_    ----
        //                    ---    | |    / _ \| '_ ` _ \| '_ ` _ \ / _` | '_ \ / _` / __| | |    | / __| __|   ----
        //                    ---    | |___| (_) | | | | | | | | | | | (_| | | | | (_| \__ \ | |____| \__ \ |_    ----
        //                    ---     \_____\___/|_| |_| |_|_| |_| |_|\__,_|_| |_|\__,_|___/ |______|_|___/\__|   ----
        //                    """);
        // content.AppendLine("\n");

        var commandMax = commands.Max(x => x.Name.Length);
        var descriptionMax = commands.Max(x => x.Description.Length);

        content.AppendLine("\u250C" + " Command ".Centralize(commandMax + 2, '\u2500') +
                           "\u252C" + " Description ".Centralize(descriptionMax + 2, '\u2500') + "\u2510");

        foreach (var command in commands)
        {
            content.AppendLine(
                $"\u2502 {command.Name.PadRight(commandMax)} " +
                $"\u2502 {command.Description.PadRight(descriptionMax)} \u2502");

            // If this is the last command, draw a bottom border
            if (command.Equals(commands.Last()))
            {
                content.AppendLine("\u2514".PadRight(commandMax + 3, '\u2500') +
                                   "\u2534".PadRight(descriptionMax + 3, '\u2500') + "\u2518");
                break;
            }

            content.AppendLine("\u251C".PadRight(commandMax + 3, '\u2500') +
                               "\u253C".PadRight(descriptionMax + 3, '\u2500') + "\u2524");
        }

        var info = await Context.Client.GetApplicationInfoAsync();

        await RespondWithFileAsync(
            text: "> ## Help\n" +
                  "### :information: Information\n" +
                  $"Bot created by {info.Owner.Mention}\n" +
                  "[Website](<https://abysmal.eu.org>) | [Github](<https://github.com/baxthus>) | [Donation](<https://dub.sh/BaxthusDonation>)\n" +
                  "### :page_facing_up: Commands",
            fileName: "commands.diff",
            fileStream: new MemoryStream(Encoding.UTF8.GetBytes(content.ToString())),
            ephemeral: true);
    }
}