using System.Net.Sockets;
using System.Text;
using Adramelech.Common;
using Adramelech.Server.Types;
using Adramelech.Utilities;
using Serilog;

namespace Adramelech.Api.Services;

public class TcpService
{
    private NetworkStream? _stream;

    public void Initialize()
    {
        var client = new TcpClient("localhost", 5000);
        _stream = client.GetStream();
    }

    public async Task<Result<Response>> SendCommandAsync(Request request)
    {
        var text = request.ToJson();
        Console.WriteLine("Sending command: {0}", text);
        // Add "command:" to the message so the server knows it's a command
        var bytes = Encoding.UTF8.GetBytes("action:" + text + "\n");
        await _stream!.WriteAsync(bytes);

        Console.WriteLine("Waiting for response");

        var buffer = new byte[1024];
        var read = await _stream.ReadAsync(buffer);
        var response = Encoding.UTF8.GetString(buffer, 0, read);

        var message = response.FromJson<Message>();
        if (message.Type != MessageType.Response)
            return new Exception("Invalid response type");

        return message.Text!.FromJson<Response>();
    }
}