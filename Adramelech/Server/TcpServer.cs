using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Adramelech.Server.Client;
using Adramelech.Server.Types;
using Adramelech.Utilities;
using Serilog;

namespace Adramelech.Server;

public class TcpServer
{
    private readonly IPEndPoint _ipep;
    private readonly TcpListener? _listener;
    private readonly List<Client.Client> _clients;
    private readonly Channel<Message> _messages;
    private readonly CancellationTokenSource _cts;

    public TcpServer(string host = "127.0.0.1", int port = 5000)
    {
        var ip = IPAddress.Parse(host);
        _ipep = new IPEndPoint(ip, port);
        _clients = new List<Client.Client>();
        _messages = Channel.CreateUnbounded<Message>();
        _cts = new CancellationTokenSource();

        var listener = ExceptionUtils.Try(() => new TcpListener(_ipep));
        if (listener.IsFailure)
        {
            Log.Error("Could not listen on {Host}:{Port}", host, port);
            return;
        }

        _listener = listener.Value;
    }

    public async Task InitializeAsync()
    {
        _listener?.Start();

        Log.Information("Listening on {Host}", _ipep.ToString());

        var messageListener = new MessageListener(_clients, _messages, _cts.Token);

        ThreadStart messageListenerTask = messageListener.HandleAsync;
        var channelListenerThread = new Thread(messageListenerTask);
        channelListenerThread.Start();

        while (!_cts.IsCancellationRequested)
        {
            var tcpClient = await ExceptionUtils.TryAsync(_listener!.AcceptTcpClientAsync);
            if (tcpClient.IsFailure)
            {
                Log.Error(tcpClient.Exception, "Could not accept client");
                continue;
            }

            var client = new Client.Client(tcpClient.Value!, _clients, _messages, _cts.Token);

            ThreadStart clientTask = client.HandleAsync;
            var clientThread = new Thread(clientTask);
            clientThread.Start();
        }
    }

    public void Stop()
    {
        _cts.Cancel();

        var tryStop = ExceptionUtils.Try(() => _listener!.Stop());
        if (tryStop.IsFailure)
            Log.Error(tryStop.Exception, "Could not stop listener");
    }
}