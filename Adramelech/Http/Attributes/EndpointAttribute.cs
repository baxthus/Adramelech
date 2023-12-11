namespace Adramelech.Http.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class EndpointAttribute(string path, string method = "GET")
    : Attribute
{
    public string Path { get; } = path;
    public string Method { get; } = method;
}