namespace Adramelech.Utilities;

/// <summary>
/// A collection of utility methods for conditional logic and flow control.
/// </summary>
public static class ConditionalUtils
{
    /// <summary>
    /// Run multiple checks on <see cref="value"/> to see if it's invalid.
    /// </summary>
    /// <param name="value">The value to check (can be implicit)</param>
    /// <typeparam name="T">The type of <see cref="value"/> (can be implicit)</typeparam>
    /// <returns>True if the value is invalid, false if it's valid</returns>
    /// <remarks>This is a very expensive method, so use it sparingly</remarks>
    public static bool IsInvalid<T>(this T value)
    {
        // Deal with normal scenarios
        if (value is null) return true;
        if (Equals(value, default(T))) return true;

        var methodType = typeof(T);

        // Deal with empty strings
        // I just implemented the whitespace check because the Github API returns a whitespace string sometimes
        if (methodType == typeof(string))
            return string.IsNullOrEmpty(value as string) || string.IsNullOrWhiteSpace(value as string);

        // Deal with empty arrays
        if (methodType.IsArray) return (value as Array)!.Length == 0;

        // Deal with non-null nullables
        if (Nullable.GetUnderlyingType(methodType) is not null) return false;

        // Deal with boxed value types
        var argumentType = value.GetType();
        if (!argumentType.IsValueType || argumentType == methodType) return false;

        // Deal with wrapped types
        var obj = Activator.CreateInstance(value.GetType())!;
        return obj.Equals(value);
    }

    /// <summary>
    /// Check if <see cref="value"/> is the default value for <see cref="T"/>
    /// </summary>
    /// <param name="value">The value to check (can be implicit)</param>
    /// <typeparam name="T">The type of <see cref="value"/> (can be implicit)</typeparam>
    /// <returns>True if the value is the default value, false if it's not</returns>
    /// <remarks>This also checks for empty arrays</remarks>
    public static bool IsDefault<T>(this T value) => Equals(value, default(T)) || value is Array { Length: 0 };

    /// <summary>
    /// Return <see cref="value"/> if it's valid, otherwise return <see cref="fallback"/>
    /// </summary>
    /// <param name="value">The value to check (can be implicit)</param>
    /// <param name="fallback">The fallback value to return if <see cref="value"/> is invalid</param>
    /// <typeparam name="T">The type of <see cref="value"/> (can be implicit)</typeparam>
    /// <seealso cref="OrElse"/>
    // /// <remarks>This calls <see cref="IsInvalid{T}"/> on the value, so it's a very expensive method</remarks>
    public static T OrElse<T>(this T value, T fallback) => value is null || value.IsDefault() ? fallback : value;

    /// <summary>
    /// Return <see cref="value"/> if it's valid, otherwise return <see cref="fallback"/>
    /// </summary>
    /// <param name="value">The value to check (can be implicit)</param>
    /// <param name="fallback">The fallback value to return if <see cref="value"/> is invalid</param>
    /// <returns>The value if it's valid, otherwise the fallback value</returns>
    /// <seealso cref="OrElse{T}"/>
    public static string OrElse(this string? value, string fallback) => string.IsNullOrEmpty(value) ? fallback : value;

    /// <summary>
    /// Return true if <see cref="value"/> is null or empty, otherwise return false
    /// </summary>
    /// <param name="value">The value to check (can be implicit)</param>
    /// <remarks>This calls <see cref="string.Trim()"/> on the value</remarks>
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value?.Trim());
}