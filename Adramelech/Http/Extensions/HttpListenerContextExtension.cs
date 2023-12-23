using System.Net;
using System.Text;
using Adramelech.Utilities;
using Serilog;

namespace Adramelech.Http.Extensions;

public static class HttpListenerContextExtension
{
    public static async Task RespondAsync(this HttpListenerContext context, ReadOnlyMemory<byte> buffer,
        HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "text/plain")
    {
        var response = context.Response;

        response.StatusCode = statusCode.GetHashCode();
        response.ContentType = contentType;
        response.ContentLength64 = buffer.Length;

        // This take me so much time to figure out. I hate asynchronous programming
        await Task.Run(async () =>
        {
            var exception = await ExceptionUtils.TryAndFinallyAsync(
                async () => await response.OutputStream.WriteAsync(buffer),
                response.Close);
            if (exception is not null)
                Log.Error(exception, "Failed to write to output stream");
        });
    }

    public static Task RespondAsync(this HttpListenerContext context, string content,
        HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "text/plain") =>
        RespondAsync(context, Encoding.UTF8.GetBytes(content), statusCode, contentType);
}