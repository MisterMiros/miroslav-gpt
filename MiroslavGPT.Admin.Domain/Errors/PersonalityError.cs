namespace MiroslavGPT.Admin.Domain.Errors;

public static class PersonalityError
{
    public const string NOT_FOUND = "personality:notfound";
    public const string ALREADY_EXISTS = "personality:alreadyexists";
    public const string EMPTY_COMMAND = "personality:emptycommand";
    public const string INVALID_COMMAND = "personality:invalidcommand";
    public const string EMPTY_MESSAGE = "personality:emptymessage";
}