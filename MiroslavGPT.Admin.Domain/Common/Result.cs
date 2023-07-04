namespace MiroslavGPT.Admin.Domain.Common;

public static class Result
{
    public static Result<TError> Ok<TError>() where TError: Enum
    {
        return Result<TError>.Ok();
    }
    
    public static Result<TValue, TError> Ok<TValue, TError>(TValue value) where TError: Enum
    {
        return Result<TValue, TError>.Ok(value);
    }

    public static Task<Result<TValue, TError>> OkAsync<TValue, TError>(Task<TValue> value) where TError: Enum
    {
        return Result<TValue, TError>.OkAsync(value);
    }
    
    public static Result<TError> Failure<TError>(TError error) where TError: Enum
    {
        return Result<TError>.Failure(error);
    }
    
    public static Result<TValue, TError> Failure<TValue, TError>(TError error) where TError: Enum
    {
        return Result<TValue, TError>.Failure(error);
    }
}