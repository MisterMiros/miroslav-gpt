namespace MiroslavGPT.Admin.Domain.Errors;

public enum PersonalityError
{
    NotFound,
    AlreadyExists,
    EmptyCommand,
    InvalidCommand,
    EmptyMessage,
}