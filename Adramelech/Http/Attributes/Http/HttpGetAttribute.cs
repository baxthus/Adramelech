namespace Adramelech.Http.Attributes.Http;

public class HttpGetAttribute(string? path = null) : HttpMethodAttribute(path)
{
    public override string Method => "GET";
}