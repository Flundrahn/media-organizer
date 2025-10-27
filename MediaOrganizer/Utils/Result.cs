namespace MediaOrganizer.Utils;

public record Result<T> : ResultBase
{
    private T? _value;

    /// <summary>
    /// If result is success which also ensures value is not null.
    /// </summary>
    public T Value => _value ?? throw new InvalidOperationException("Value is null, must check to ensure success before getting value.");

    private Result() { }

    private Result(T value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
        IsSuccess = true;
    }

    public static Result<T> Success(T value) => new Result<T>(value);

    public static new Result<T> Failure(string error) => new Result<T> { IsSuccess = false, Error = error };
}

// TODO: cleanup if only use generic result type
public record ResultBase
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public static ResultBase Success() => new ResultBase { IsSuccess = true };
    public static ResultBase Failure(string error) => new ResultBase { IsSuccess = false, Error = error };
}
