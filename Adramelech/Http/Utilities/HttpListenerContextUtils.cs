using System.Net;
using System.Text;
using Adramelech.Utilities;

namespace Adramelech.Http.Utilities;

public static class HttpListenerContextUtils
{
    public static async Task RespondAsync(this HttpListenerContext context, ReadOnlyMemory<byte> buffer,
        HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "text/plain")
    {
        var response = context.Response;

        response.StatusCode = statusCode.GetHashCode();
        response.ContentType = contentType;
        response.ContentLength64 = buffer.Length;

        await Task.Run(async () =>
        {
            await ExceptionUtils.TryAndFinallyAsync(
                async () => await response.OutputStream.WriteAsync(buffer),
                response.Close);
        });
    }

    public static Task RespondAsync(this HttpListenerContext context, string content,
        HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "text/plain") =>
        RespondAsync(context, Encoding.UTF8.GetBytes(content), statusCode, contentType);
}