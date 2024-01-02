namespace Adramelech.Utilities;

/// <summary>
/// Utilities for dependency injection
/// </summary>
public static class DiUtils
{
    /// <summary>
    /// Get first instance of <typeparamref name="T"/> from <paramref name="provider"/>
    /// </summary>
    /// <param name="provider">The <see cref="IServiceProvider"/> to get the instance from</param>
    /// <typeparam name="T">The type of the instance to get</typeparam>
    /// <returns>The instance of <typeparamref name="T"/> or <see langword="null"/> if not found</returns>
    public static T? GetService<T>(this IServiceProvider provider) => (T?)provider.GetService(typeof(T));
}