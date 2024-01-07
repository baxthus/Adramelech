using System.Threading.Channels;
using Adramelech.Server.Common;
using Adramelech.Server.Types;

namespace Adramelech.Server.Actions.Files;

public class List(
    Message message,
    Request request,
    List<Client.Client> clients,
    Channel<Message> messages,
    CancellationToken cancellationToken) : ActionBase(message, request, clients, messages, cancellationToken)
{
    public override string Name => "files-list";

    public override Task HandleAsync()
    {
        Write("Hello from List");
        return Task.CompletedTask;
    }
}