using Adramelech.Configuration;
using Discord;
using Discord.WebSocket;

namespace Adramelech.Http.Utilities;

public static class FilesEndpointUtils
{
    public static async Task<SocketTextChannel?> GetChannel(DiscordSocketClient botClient)
    {
        var channelId = HttpConfig.Instance.FilesChannel;
        if (channelId == null)
            return null;

        var channel = await botClient.GetChannelAsync(channelId.Value);
        return channel as SocketTextChannel;
    }

    public static async Task<(List<IMessage>, bool)> GetAllMessages(this SocketTextChannel channel,
        IEnumerable<ulong> ids)
    {
        var messages = new List<IMessage>();
        var missing = false;

        foreach (var id in ids)
        {
            var message = await channel.GetMessageAsync(id);
            if (message is null)
            {
                missing = true;
                continue;
            }

            messages.Add(message);
        }

        return (messages, missing);
    }
}