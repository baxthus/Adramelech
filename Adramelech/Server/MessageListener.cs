using System.Threading.Channels;
using Adramelech.Server.Common;
using Adramelech.Server.Types;
using Adramelech.Utilities;
using Serilog;

namespace Adramelech.Server;

public class MessageListener(
    ICollection<Client.Client> clients,
    Channel<Message, Message> messages,
    CancellationToken cancellationToken)
{
    public async void HandleAsync()
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var message = await messages.Reader.ReadAsync(cancellationToken);
            var client = clients.FirstOrDefault(c => c.TcpClient == message.Client)!;

            switch (message.Type)
            {
                case MessageType.Connected:
                    Log.Information("Client {Client} connected", client.Ip);
                    break;
                case MessageType.Disconnected:
                    client.Close();
                    clients.Remove(client);
                    Log.Information("Client {Client} disconnected", client.Ip);
                    break;
                case MessageType.Request:
                    Log.Information("Received request from {Client}", client.Ip);
                    var request = message.Text?.FromJson<Request>()!;

                    var action = ReflectionUtils
                        .GetInstances<ActionBase>(message, request, clients, messages, cancellationToken)
                        .FirstOrDefault(a => a.Name == request?.Action);
                    if (action is null)
                    {
                        var response = new Response(false, "Unknown action");
                        messages.Writer.TryWrite(new Message(MessageType.Response, message.Client, response.ToJson()));
                        return;
                    }

                    Log.Debug("Handling request {Request} from {Client}", request, client.Ip);

                    await action.HandleAsync();

                    break;
                case MessageType.Response:
                    await client.Writer.WriteAsync(message.Text);
                    break;
                default:
                    Log.Warning("Unknown message type {Type} from {Client}", message.Type,
                        client.Ip);
                    break;
            }
        }
    }
}