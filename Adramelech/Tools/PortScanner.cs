using System.Net.Sockets;
using Adramelech.Data;
using Adramelech.Logging;
using Discord.WebSocket;
using Serilog;

namespace Adramelech.Tools;

public class PortScanner
{
    private readonly string _host;
    private readonly List<int> _ports;
    private readonly ILogger _logger;
    private readonly CancellationToken _cancellationToken;

    public PortScanner(string host, List<int> ports, SocketUser user, CancellationToken cancellationToken = default)
    {
        // Verify if any ports are out of range (1-65535)
        if (ports.Any(port => port is < 1 or > 65535))
            throw new ArgumentException("Ports must be between 1 and 65535");

        // Verify if the host is a private IP address or domain
        if (BadAddresses.Hosts.Any(host.StartsWith) || BadAddresses.TlDs.Any(host.EndsWith))
            throw new ArgumentException("Private or reserved IP addresses and domains are not allowed");

        _logger = Loggers.UserContext.ForContext("User", user);

        _logger.Debug("User started a port scan on {Host}", host);

        _host = host;
        _ports = ports;
        _cancellationToken = cancellationToken;
    }

    public async Task<List<int>> ScanAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            var openPorts = new List<int>();

            foreach (var port in _ports)
            {
                using var tcpClient = new TcpClient();

                try
                {
                    await tcpClient.ConnectAsync(_host, port, _cancellationToken);
                    openPorts.Add(port);
                    _logger.Debug("Port {Port} is open", port);
                }
                catch (SocketException)
                {
                    _logger.Debug("Port {Port} is closed", port);
                }
            }

            _logger.Debug("Port scan on {Host} complete", _host);

            return openPorts;
        }

        throw new TaskCanceledException();
    }
}