using Adramelech.Configuration;
using Discord.WebSocket;

namespace Adramelech.Http.Utilities;

public class FilesEndpointUtils
{
    public static async Task<SocketTextChannel?> GetChannel(DiscordSocketClient botClient)
    {
        var channelId = HttpConfig.Instance.FilesChannel;
        if (channelId == null)
            return null;

        var channel = await botClient.GetChannelAsync(channelId.Value);
        return channel as SocketTextChannel;
    }

    public static string GetExtension(string contentType) =>
        MimeMapping.MimeUtility.GetExtensions(contentType).FirstOrDefault() ?? "txt";
}