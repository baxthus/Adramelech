namespace Adramelech.Http.Attributes;

public class HttpPostAttribute(string? path = null) : HttpMethodAttribute(path)
{
    public override string Method => "POST";
}