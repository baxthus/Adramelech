namespace Adramelech.Http.Attributes.Middleware;

[AttributeUsage(AttributeTargets.Class)]
public class NeedsTokenAttribute(TokenSources priority = TokenSources.QueryString) : Attribute
{
    public readonly TokenSources Priority = priority;
}

public enum TokenSources
{
    Cookie,
    Header,
    QueryString
}