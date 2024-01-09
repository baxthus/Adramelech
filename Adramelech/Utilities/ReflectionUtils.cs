using System.Reflection;

namespace Adramelech.Utilities;

/// <summary>
/// A collection of reflection utilities.
/// </summary>
public static class ReflectionUtils
{
    /// <summary>
    /// Get all instances of a class
    /// </summary>
    /// <typeparam name="T">The type of the class</typeparam>
    /// <returns>An enumerable of all instances of the class</returns>
    /// <remarks>Can throw a lot of exceptions</remarks>
    public static IEnumerable<T> GetInstances<T>(params object[] args) where T : class =>
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(T)))
            .Select(t => (T)Activator.CreateInstance(t, args)!);

    public static IEnumerable<T> GetAttributes<T>(object obj) where T : Attribute =>
        obj.GetType().GetCustomAttributes(typeof(T), false).Select(a => (T)a);

    public static IEnumerable<KeyValuePair<T, MethodInfo>> GetMethodsFromAttribute<T>(object obj, bool inherit = false)
        where T : Attribute =>
        obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(m => new KeyValuePair<T, MethodInfo>(m.GetCustomAttribute<T>(inherit)!, m))
            .Where(_ => true);
}