using System.Text;
using Serilog;

namespace Adramelech.Utilities;

/// <summary>
/// Utility class for HTTP requests
/// </summary>
public static class HttpUtils
{
    /// <summary>
    /// GET request
    /// </summary>
    /// <param name="url">The URL to send the request to</param>
    /// <param name="userAgent">The user agent to use (optional)</param>
    /// <typeparam name="T">The type of the response</typeparam>
    /// <returns>The response of the request; default if the request failed</returns>
    /// <remarks>This method don't support returning a <see cref="int"/>, because parsing the response to an <see cref="int"/> is unnecessary work</remarks>
    /// <seealso cref="Request{T,TF}"/>
    public static async Task<T?> Request<T>(this string url, string? userAgent = null)
    {
        using HttpClient client = new();

        if (userAgent != null)
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        var response = await client.GetAsync(url);
        if (!response!.IsSuccessStatusCode)
            return default;

        var content = await response.Content.ReadAsStringAsync();

        // If T is string, don't deserialize
        return typeof(T) == typeof(string) ? (T)(object)content : content.FromJson<T>();
    }

    /// <summary>
    /// POST request
    /// </summary>
    /// <param name="url">The URL to send the request to</param>
    /// <param name="data">The data to send</param>
    /// <param name="userAgent">The user agent to use (optional)</param>
    /// <typeparam name="T">The type of the data to send</typeparam>
    /// <typeparam name="TF">The type of the response</typeparam>
    /// <returns>The response of the request; default if the request failed</returns>
    /// <remarks>This method don't support returning a <see cref="int"/>, because parsing the response to an <see cref="int"/> is unnecessary work</remarks>
    /// <seealso cref="Request{T}"/>
    public static async Task<TF?> Request<T, TF>(this string url, T data, string? userAgent = null)
    {
        using HttpClient client = new();

        if (userAgent is not null)
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        // If the content is a string, don't serialize
        using var httpContent = typeof(T) == typeof(string)
            ? new StringContent((string)(object)data!, Encoding.UTF8, "text/plain")
            : new StringContent(data!.ToJson(), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, httpContent);
        if (!response.IsSuccessStatusCode)
            return default;

        var content = await response.Content.ReadAsStringAsync();

        // If TF is string, don't deserialize
        return typeof(TF) == typeof(string) ? (TF)(object)content : content.FromJson<TF>();
    }
}