namespace Adramelech.Http.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NeedsTokenAttribute(bool needsToken = true) : Attribute
{
    public bool NeedsToken { get; } = needsToken;
}