namespace MiroslavGPT.Admin.Domain.Common;

public record Result<TValue, TError> where TError : Enum
{
    public TValue? Value { get; init; } = default!;
    public bool Success { get; init; } = true;
    public TError? Error { get; init; }
    
    
    public static Result<TValue, TError> Ok(TValue value)
    {
        return new Result<TValue, TError>
        {
            Value = value,
            Success = true,
        };
    }

    public static async Task<Result<TValue, TError>> OkAsync(Task<TValue> value)
    {
        return new Result<TValue, TError>
        {
            Value = await value,
            Success = true,
        };
    }
    
    public static Result<TValue, TError> Failure(TError error)
    {
        return new Result<TValue, TError>
        {
            Value = default,
            Success = false,
            Error = error,
        };
    }
}