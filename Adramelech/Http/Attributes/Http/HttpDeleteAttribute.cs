namespace Adramelech.Http.Attributes.Http;

public class HttpDeleteAttribute(string? path = null) : HttpMethodAttribute(path)
{
    public override string Method => "DELETE";
}