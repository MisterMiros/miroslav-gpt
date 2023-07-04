namespace MiroslavGPT.Admin.Domain.Common;

public record Result<TError> where TError : Enum
{
    public bool Success { get; init; } = true;
    public TError? Error { get; init; }
    
    public static Result<TError> Ok()
    {
        return new Result<TError>
        {
            Success = true,
        };
    }
    
    public static Result<TError> OkAsync()
    {
        return new Result<TError>
        {
            Success = true,
        };
    }

    public static Result<TError> Failure(TError error)
    {
        return new Result<TError>
        {
            Success = false,
            Error = error,
        };
    }
}