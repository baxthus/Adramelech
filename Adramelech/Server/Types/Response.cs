namespace Adramelech.Server.Types;

public struct Response(bool success, string? message = null, object? data = null)
{
    public bool Success = success;
    public string? Message = message;
    public object? Data = data;
}