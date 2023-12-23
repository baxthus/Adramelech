namespace Adramelech.Utilities;

/// <summary>
/// Utilities for handling exceptions
/// </summary>
public static class ExceptionUtils
{
    /// <summary>
    /// Invoke a function and return the result or the exception thrown
    /// </summary>
    /// <param name="func">The function to invoke</param>
    /// <typeparam name="T">The type of the result (can be inferred)</typeparam>
    /// <returns>The tuple of the result and the exception thrown</returns>
    public static (T?, Exception?) Try<T>(this Func<T> func)
    {
        try
        {
            return (func(), null);
        }
        catch (Exception e)
        {
            return (default, e);
        }
    }

    /// <summary>
    /// Invoke a function and return the exception thrown
    /// </summary>
    /// <param name="func">The function to invoke</param>
    /// <returns>The exception thrown</returns>
    public static Exception? Try(this Action func)
    {
        try
        {
            func();
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    /// Invoke a function and return the result or the exception thrown
    /// </summary>
    /// <param name="func">The function to invoke</param>
    /// <typeparam name="T">The type of the result (can be inferred)</typeparam>
    /// <returns>The tuple of the result and the exception thrown</returns>
    public static async Task<(T?, Exception?)> TryAsync<T>(this Func<Task<T>> func)
    {
        try
        {
            return (await func(), null);
        }
        catch (Exception e)
        {
            return (default, e);
        }
    }

    private static async Task<Exception?> InternalTryVoid(this Func<Task> func, Func<Task>? finallyFunc = null)
    {
        try
        {
            await func();
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
        finally
        {
            if (finallyFunc is not null)
                await finallyFunc();
        }
    }

    /// <summary>
    /// Invoke a async function and return the exception thrown
    /// </summary>
    /// <param name="func">The function to invoke</param>
    /// <returns>The exception thrown</returns>
    public static async Task<Exception?> TryAsync(this Func<Task> func) => await InternalTryVoid(func);

    /// <summary>
    /// Invoke a async function and return the exception thrown, and invoke a function after the function is invoked
    /// </summary>
    /// <param name="func">The function to invoke</param>
    /// <param name="finallyFunc">The function to invoke after the function is invoked</param>
    /// <returns>The exception thrown</returns>
    public static async Task<Exception?> TryAndFinallyAsync(this Func<Task> func, Action? finallyFunc = null) =>
        await InternalTryVoid(func, finallyFunc is null
            ? null
            : async () =>
            {
                finallyFunc();
                await Task.CompletedTask;
            });
}