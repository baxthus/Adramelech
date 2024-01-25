namespace Adramelech.Http.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RouteAttribute(string path) : Attribute
{
    public readonly string Path = path.StartsWith('/') ? path : $"/{path}";
}