namespace MiroslavGPT.Admin.Domain.Common;

public record Result<TValue>
{
    public TValue? Value { get; init; }
    public bool Success { get; init; } = true;
    public string? Error { get; init; }
    
    
    public static Result<TValue> Ok(TValue value)
    {
        return new Result<TValue>
        {
            Value = value,
            Success = true,
        };
    }

    public static async Task<Result<TValue>> OkAsync(Task<TValue> value)
    {
        return new Result<TValue>
        {
            Value = await value,
            Success = true,
        };
    }
    
    public static Result<TValue> Failure(string error)
    {
        return new Result<TValue>
        {
            Value = default,
            Success = false,
            Error = error,
        };
    }
}