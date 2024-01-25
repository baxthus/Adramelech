using Adramelech.Http.Server;
using Adramelech.Services;
using Discord.WebSocket;

namespace Adramelech.Http;

public class HttpWrapper(DatabaseService dbService, ConfigService configService, DiscordSocketClient botClient)
{
    public void Initialize()
    {
        var server = new HttpServer();

        server.AddControllers();
        server.AddMiddlewares();

        server.AddDependency(dbService);
        server.AddDependency(configService);
        server.AddDependency(botClient);

        server.Serve(configService.Port);
    }
}