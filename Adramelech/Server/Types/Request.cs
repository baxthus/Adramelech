namespace Adramelech.Server.Types;

public struct Request(string action, string[] arguments)
{
    public string Action = action;
    public string[] Arguments = arguments;
}