namespace Adramelech.Common;

public class Result
{
    public bool Success { get; }
    public bool IsFailure => !Success;
    public Exception? Exception { get; private set; }

    protected Result(bool success, Exception? exception)
    {
        switch (success)
        {
            case true when exception is not null:
                throw new InvalidOperationException("Cannot be successful and have an exception");
            case false when exception is null:
                throw new InvalidOperationException("Cannot be unsuccessful and not have an exception");
            default:
                Success = success;
                Exception = exception;
                break;
        }
    }

    public static Result Fail(Exception exception) => new(false, exception);
    public static Result<T> Fail<T>(Exception exception) => new(default, false, exception);
    public static Result Ok() => new(true, null);
    public static Result<T> Ok<T>(T? value) => new(value, true, null);
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected internal Result(T? value, bool success, Exception? exception) : base(success, exception)
    {
        Value = value;
    }
}