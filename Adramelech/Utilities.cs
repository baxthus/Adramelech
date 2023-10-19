using System.Text;
using Discord;
using Newtonsoft.Json;

namespace Adramelech;

/// <summary>
/// A collection of utility methods
/// </summary>
public static class Utilities
{
    /// <summary>
    /// Respond with a ephemeral error message
    /// </summary>
    /// <param name="ctx">The interaction context (can be implicit)</param>
    /// <param name="desc">The description of the error (optional)</param>
    /// <param name="isDeferred">Whether the response is deferred or not</param>
    /// <remarks>This is just to make the code look cleaner</remarks>
    public static async Task ErrorResponse(this IInteractionContext ctx, string? desc = null, bool isDeferred = false)
    {
        var embed = desc == null
            ? new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("**Error!**")
            : new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("**Error!**")
                .WithDescription(desc);

        if (isDeferred)
            await ctx.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: true);
        else
            await ctx.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    /// <summary>
    /// Serialize an object to JSON
    /// </summary>
    /// <param name="obj">The object to serialize (can be implicit)</param>
    /// <returns>The serialized string of the object</returns>
    /// <seealso cref="FromJson{T}"/>
    public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj);

    /// <summary>
    /// Deserialize a JSON string to an object
    /// </summary>
    /// <param name="json">The JSON string to deserialize (can be implicit)</param>
    /// <typeparam name="T">The type of the object to deserialize to</typeparam>
    /// <returns>The deserialized object</returns>
    /// <seealso cref="ToJson"/>
    public static T? FromJson<T>(this string json) => JsonConvert.DeserializeObject<T>(json);

    /// <summary>
    /// GET request
    /// </summary>
    /// <param name="url">The URL to send the request to</param>
    /// <param name="userAgent">The user agent to use (optional)</param>
    /// <typeparam name="T">The type of the data to receive</typeparam>
    /// <returns >The response from the server; default if the request failed</returns>
    /// <remarks>This method don't support returning a number, because parsing a number from a string is unnecessary work</remarks>
    /// <seealso cref="Request{T,TF}"/>
    public static async Task<T?> Request<T>(string url, string? userAgent = null)
    {
        using HttpClient client = new();

        if (userAgent != null)
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return default;

        var content = await response.Content.ReadAsStringAsync();

        // If T is string, return string
        return typeof(T) == typeof(string) ? (T)(object)content : content.FromJson<T>();
    }

    /// <summary>
    ///  POST request
    /// </summary>
    /// <param name="url">The URL to send the request to</param>
    /// <param name="data">The data to send</param>
    /// <param name="userAgent">The user agent to use (optional)</param>
    /// <typeparam name="T">The type of the data to send</typeparam>
    /// <typeparam name="TF">The type of the data to receive</typeparam>
    /// <returns>The response from the server; default if the request failed</returns>
    /// <remarks>This method don't support returning a number, because parsing a number from a string is unnecessary work</remarks>
    /// <seealso cref="Request{T}"/>
    public static async Task<TF?> Request<T, TF>(string url, T data, string? userAgent = null)
    {
        using HttpClient client = new();

        if (userAgent != null)
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        // If the content is string, theres no need to serialize it
        using var httpContent = typeof(TF) == typeof(string)
            ? new StringContent((string)(object)data!, Encoding.UTF8, "text/plain")
            : new StringContent(data!.ToJson(), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, httpContent);
        if (!response.IsSuccessStatusCode)
            return default;

        var content = await response.Content.ReadAsStringAsync();

        // If TF is string, return string
        return typeof(TF) == typeof(string) ? (TF)(object)content : content.FromJson<TF>();
    }

    /// <summary>Run multiple checks on a value to see if it's invalid.</summary>
    /// <param name="value">The value to check (can be implicit)</param>
    /// <returns>True if the value is invalid, false if it's valid</returns>
    /// <remarks>This is a very expensive method, so use it sparingly.</remarks>
    public static bool IsInvalid<T>(this T value)
    {
        // Deal with normal scenarios
        if (value == null) return true;
        if (Equals(value, default(T))) return true;

        var methodType = typeof(T);

        // Deal with empty strings
        // I just implemented the whitespace check because the Github API returns a whitespace string sometimes
        if (methodType == typeof(string))
            return string.IsNullOrEmpty(value as string) || string.IsNullOrWhiteSpace(value as string);

        // Deal with empty arrays
        if (methodType.IsArray) return (value as Array)!.Length == 0;

        // Deal with non-null nullables
        if (Nullable.GetUnderlyingType(methodType) != null) return false;

        // Deal with boxed value types
        var argumentType = value.GetType();
        if (!argumentType.IsValueType || argumentType == methodType) return false;

        // Deal with wrapped types
        var obj = Activator.CreateInstance(value.GetType())!;
        return obj.Equals(value);
    }

    /// <summary>Return the value if it's not null, otherwise return the fallback</summary>
    /// <param name="value">The value to check (can be implicit)</param>
    /// <param name="fallback">The fallback value to return if the value is null</param>
    /// <remarks>This calls <see cref="IsInvalid{T}"/> on the value, so it's expensive.</remarks>
    public static T OrElse<T>(this T value, T fallback) => value.IsInvalid() ? fallback : value;

    /// <summary>Capitalizes the first letter of a string</summary>
    /// <param name="text">The string to capitalize (can be implicit)</param>
    /// <remarks>Why is this not a built-in method?</remarks>
    public static string Capitalize(this string text) => text.Length switch
    {
        0 => text,
        1 => text.ToUpper(),
        _ => text[0].ToString().ToUpper() + text[1..]
    };
}