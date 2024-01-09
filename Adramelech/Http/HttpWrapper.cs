using Adramelech.Http.Server;
using Adramelech.Services;
using Discord.WebSocket;

namespace Adramelech.Http;

public class HttpWrapper(DiscordSocketClient botClient, ConfigService configService)
{
    public void Initialize()
    {
        var server = new HttpServer();

        server.AddControllers();
        server.AddMiddlewares();

        server.AddDependency(botClient);

        server.Serve(configService.BridgePort);
    }
}