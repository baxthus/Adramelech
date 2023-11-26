using System.Text;
using Discord.Interactions;
using Discord.WebSocket;

namespace Adramelech.Commands;

public class Help : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("help", "Get help with the bot")]
    public async Task HelpAsync()
    {
        var commands = await Context.Client.GetGlobalApplicationCommandsAsync();

        StringBuilder commandsString = new();

        commandsString.AppendLine("""
                                  ---      _____                                          _       _      _     _      ----
                                  ---     / ____|                                        | |     | |    (_)   | |     ----
                                  ---    | |     ___  _ __ ___  _ __ ___   __ _ _ __   __| |___  | |     _ ___| |_    ----
                                  ---    | |    / _ \| '_ ` _ \| '_ ` _ \ / _` | '_ \ / _` / __| | |    | / __| __|   ----
                                  ---    | |___| (_) | | | | | | | | | | | (_| | | | | (_| \__ \ | |____| \__ \ |_    ----
                                  ---     \_____\___/|_| |_| |_|_| |_| |_|\__,_|_| |_|\__,_|___/ |______|_|___/\__|   ----
                                  """);
        commandsString.AppendLine("\n");

        var alignment = commands.Max(x => x.Name.Length);

        foreach (var command in commands)
            commandsString.AppendLine($"{command.Name.PadRight(alignment)} - {command.Description}");

        await RespondWithFileAsync(
            fileName: "commands.diff",
            fileStream: new MemoryStream(Encoding.UTF8.GetBytes(commandsString.ToString())),
            ephemeral: true);
    }
}