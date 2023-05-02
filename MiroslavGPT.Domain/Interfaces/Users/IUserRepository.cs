namespace MiroslavGPT.Domain.Interfaces.Users
{
    public interface IUserRepository
    {
        Task<bool> IsAuthorizedAsync(long userId);
        Task AuthorizeUserAsync(long userId);
    }
}