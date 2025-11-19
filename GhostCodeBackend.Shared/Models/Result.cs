namespace GhostCodeBackend.Shared.Models;


// Result pattern, lol
public readonly record struct Result
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}

public readonly record struct Result<T>
{
    public T? Value { get; init; }
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);

    public T GetValueOrThrow() =>
        IsSuccess ? Value! : throw new InvalidOperationException(Error);
}