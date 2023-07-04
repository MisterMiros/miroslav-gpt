namespace MiroslavGPT.Admin.Domain.Common;

public record Result
{
    public bool Success { get; init; } = true;
    public string? Error { get; init; }
    
    
    public static Result Ok()
    {
        return new Result
        {
            Success = true,
        };
    }
    
    public static Result Failure(string error)
    {
        return new Result
        {
            Success = false,
            Error = error,
        };
    }
}