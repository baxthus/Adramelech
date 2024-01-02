using Discord.WebSocket;

namespace Adramelech.Http;

/// <summary>
/// Represents a HTTP server
/// </summary>
public class HttpServer(DiscordSocketClient botClient, int port = 8000)
{
    public async Task InitializeAsync()
    {
        var server = new Server.HttpServer();

        server.AddEndpoints();
        server.AddMiddlewares();

        server.AddDependency(botClient);

        await server.ServeAsync(port);
    }
}