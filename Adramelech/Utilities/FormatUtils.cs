namespace Adramelech.Utilities;

/// <summary>
/// Utilities for formatting <see cref="string"/>s
/// </summary>
public static class FormatUtils
{
    /// <summary>
    /// Capitalizes the first letter of a <see cref="string"/>
    /// </summary>
    /// <param name="text">The <see cref="string"/> to capitalize (can be implicit)</param>
    /// <returns>The capitalized <see cref="string"/></returns>
    public static string Capitalize(this string text) => text.Length switch
    {
        0 => text,
        1 => text.ToUpper(),
        _ => text[0].ToString().ToUpper() + text[1..]
    };
}