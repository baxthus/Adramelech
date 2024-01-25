namespace Adramelech.Http.Attributes;

public class HttpDeleteAttribute(string? path = null) : HttpMethodAttribute(path)
{
    public override string Method => "DELETE";
}