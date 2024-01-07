using System.Threading.Channels;
using Adramelech.Server.Types;

namespace Adramelech.Server.Common;

public abstract class ActionBase(
    Message message,
    Request request,
    List<Client.Client> clients,
    Channel<Message> messages,
    CancellationToken cancellationToken)
{
    public abstract string Name { get; }

    protected readonly List<Client.Client> Clients = clients;
    protected readonly Message Message = message;
    protected readonly Request Request = request;
    protected readonly Channel<Message> Messages = messages;
    protected readonly CancellationToken CancellationToken = cancellationToken;

    public abstract Task HandleAsync();

    protected void Write(string text) =>
        Messages.Writer.TryWrite(new Message(MessageType.Response, Message.Client, text));
}