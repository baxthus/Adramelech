using System.Text;
using Discord;
using Discord.Interactions;
using Newtonsoft.Json;

namespace Adramelech;

public static class Utilities
{
    public static async Task ErrorResponse(this SocketInteractionContext ctx, string? desc = null)
    {
        var embed = desc == null
            ? new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("**Error!**")
            : new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("**Error!**")
                .WithDescription(desc);

        await ctx.Interaction.RespondAsync(embed: embed.Build());
    }

    // GET request
    public static async Task<T?> Request<T>(string url, string? userAgent = null)
    {
        using HttpClient client = new();

        if (userAgent != null)
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return default;

        // If T is string, return string
        return typeof(T) == typeof(string)
            ? (T)(object)await response.Content.ReadAsStringAsync()
            : JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
    }

    // POST request (T type can be set implicitly by the data parameter (recommended))
    public static async Task<TF?> Request<TF, T>(string url, T data, string? userAgent = null)
    {
        using HttpClient client = new();

        if (userAgent != null)
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        using var content = typeof(TF) == typeof(string)
            ? new StringContent((string)(object)data!, Encoding.UTF8, "text/plain")
            : new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
            return default;

        // If TF is string, return string
        return typeof(TF) == typeof(string)
            ? (TF)(object)await response.Content.ReadAsStringAsync()
            : JsonConvert.DeserializeObject<TF>(await response.Content.ReadAsStringAsync());
    }

    public static bool IsInvalid<T>(this T value)
    {
        // Deal with normal scenarios
        if (value == null) return true;
        if (Equals(value, default(T))) return true;

        // Deal with non-null nullables
        var methodType = typeof(T);
        if (Nullable.GetUnderlyingType(methodType) != null) return false;

        // Deal with boxed value types
        var argumentType = value.GetType();
        if (!argumentType.IsValueType || argumentType == methodType) return false;

        // Deal with empty strings
        if (methodType == typeof(string)) return string.IsNullOrEmpty(value as string);
        
        // Deal with empty arrays
        if (methodType.IsArray) return (value as Array)!.Length == 0;

        // Deal with wrapped types
        var obj = Activator.CreateInstance(value.GetType())!;
        return obj.Equals(value);
    }

    public static T OrElse<T>(this T value, T fallback)
    {
        return value.IsInvalid() ? fallback : value;
    }

    public static string Capitalize(this string text)
    {
        return text.Length switch
        {
            0 => text,
            1 => text.ToUpper(),
            _ => text[0].ToString().ToUpper() + text[1..]
        };
    }
}