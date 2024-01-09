namespace Adramelech.Http.Attributes;

public class HttpGetAttribute(string? path = null) : HttpMethodAttribute(path)
{
    public override string Method => "GET";
}