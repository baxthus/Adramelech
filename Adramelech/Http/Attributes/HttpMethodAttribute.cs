namespace Adramelech.Http.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public abstract class HttpMethodAttribute(string? path) : Attribute
{
    public abstract string Method { get; }
    public readonly string? Path = path;
}