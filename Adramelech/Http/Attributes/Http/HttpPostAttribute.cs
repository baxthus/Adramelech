namespace Adramelech.Http.Attributes.Http;

public class HttpPostAttribute(string? path = null) : HttpMethodAttribute(path)
{
    public override string Method => "POST";
}