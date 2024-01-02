using System.Net;
using Adramelech.Http.Attributes;
using Adramelech.Http.Extensions;
using Adramelech.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Adramelech.Http.Server;

/// <summary>
/// The base class for all endpoints.
/// </summary>
public abstract class EndpointBase
{
    protected HttpListenerContext Context = null!;
    protected HttpListenerRequest Request = null!;
    protected ServiceProvider ServiceProvider = null!;

    public readonly string? Path;
    private readonly string? _method;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointBase" /> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Endpoint attribute not found</exception>
    /// <remarks>Throws the same as <see cref="ReflectionUtils.GetAttributes{T}"/>.</remarks>
    protected EndpointBase()
    {
        if (ReflectionUtils.GetAttributes<EndpointAttribute>(this).FirstOrDefault() is not { } endpoint)
            throw new InvalidOperationException("Endpoint attribute not found");

        Path = endpoint.Path;
        _method = endpoint.Method;
    }

    /// <summary>
    /// Handles the request asynchronously.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="request">The request.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public async Task HandleRequestAsync(HttpListenerContext context, HttpListenerRequest request,
        ServiceProvider serviceProvider)
    {
        Context = context;
        Request = request;
        ServiceProvider = serviceProvider;

        var valid = await ExceptionUtils.TryAsync(ExecuteCheckAsync);
        if (valid.IsFailure)
        {
            Log.Error(valid.Exception, "Failed to execute check");
            await context.RespondAsync("Failed to execute check", HttpStatusCode.InternalServerError);
            return;
        }

        if (!valid.Value) return;

        var handle = await ExceptionUtils.TryAsync(HandleAsync);
        if (handle.IsFailure)
        {
            Log.Error(handle.Exception, "Failed to handle request");
            await context.RespondAsync("Internal server error", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// The method to handle the request.
    /// </summary>
    /// <returns>A task.</returns>
    /// <remarks>Can throw anything.</remarks>
    protected abstract Task HandleAsync();

    /// <summary>
    /// Gets the body of the request.
    /// </summary>
    /// <returns>The body.</returns>
    /// <exception cref="InvalidOperationException">Body is empty</exception>
    /// <remarks>Throws the same as <see cref="BinaryReader.ReadBytes(int)"/> and <see cref="Convert.ToInt32(long)"/>.</remarks>
    internal byte[] GetBody()
    {
        using BinaryReader r = new(Request.InputStream);
        var buffer = r.ReadBytes(Convert.ToInt32(Request.ContentLength64));

        return buffer.Length > 0 ? buffer : throw new InvalidOperationException("Body is empty");
    }

    /// <summary>
    /// Executes the check's for the request.
    /// </summary>
    /// <returns>True if the request is valid, false otherwise.</returns>
    private async Task<bool> ExecuteCheckAsync()
    {
        if (_method == Request.HttpMethod) return true;

        await Context.RespondAsync("Method Not Allowed", HttpStatusCode.MethodNotAllowed);
        return false;
    }
}