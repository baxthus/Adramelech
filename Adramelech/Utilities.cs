using System.Text;
using Discord;
using Newtonsoft.Json;

namespace Adramelech;

public static class Utilities
{
    public static async Task ErrorResponse(this IInteractionContext ctx, string? desc = null)
    {
        var embed = desc == null
            ? new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("**Error!**")
            : new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("**Error!**")
                .WithDescription(desc);

        await ctx.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj);
    public static T? FromJson<T>(this string json) => JsonConvert.DeserializeObject<T>(json);

    // GET request
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

    // POST request
    public static async Task<TF?> Request<T, TF>(string url, T data, string? userAgent = null)
    {
        using HttpClient client = new();

        if (userAgent != null)
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

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