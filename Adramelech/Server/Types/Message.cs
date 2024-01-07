using System.Net.Sockets;

namespace Adramelech.Server.Types;

public struct Message(MessageType type, TcpClient? client, string? text = null)
{
    public readonly MessageType Type = type;
    public readonly TcpClient? Client = client;
    public readonly string? Text = text;
}

public enum MessageType
{
    Connected,
    Disconnected,
    Request,
    Response
}