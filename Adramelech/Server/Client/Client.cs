using System.Net.Sockets;
using System.Threading.Channels;
using Adramelech.Server.Types;
using Adramelech.Utilities;
using Serilog;

namespace Adramelech.Server.Client;

public class Client
{
    public readonly TcpClient TcpClient;
    public readonly StreamWriter Writer;
    private readonly StreamReader _reader;
    private readonly Channel<Message> _messages;
    private readonly CancellationToken _cancellationToken;
    public readonly string Ip;

    public Client(TcpClient tcpClient, ICollection<Client> clients, Channel<Message> messages,
        CancellationToken cancellationToken)
    {
        TcpClient = tcpClient;
        _messages = messages;
        _cancellationToken = cancellationToken;

        Ip = TcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";

        var stream = TcpClient.GetStream();

        Writer = new StreamWriter(stream);
        _reader = new StreamReader(stream);

        clients.Add(this);

        _messages.Writer.TryWrite(new Message(MessageType.Connected, tcpClient));
    }

    public async void HandleAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            var line = await ExceptionUtils.TryAsync(async () => await _reader.ReadLineAsync(_cancellationToken));
            if (line.IsFailure)
            {
                Log.Error(line.Exception, "Could not read from client {Ip}", Ip);
                _messages.Writer.TryWrite(new Message(MessageType.Disconnected, TcpClient));
                return;
            }

            var text = line.Value;

            Log.Debug("Received message from {Ip}: {Text}", Ip, text);

            if (string.IsNullOrWhiteSpace(text) || !text.StartsWith("action:"))
                continue;

            Log.Debug("Sending message to channel: {Text}", text[7..]);

            _messages.Writer.TryWrite(new Message(MessageType.Request, TcpClient, text[7..]));
        }
    }

    public void Close() => TcpClient.Close();
}