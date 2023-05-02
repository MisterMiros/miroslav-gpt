namespace MiroslavGPT.Domain.Interfaces.Users
{
    public interface IUsersRepository
    {
        Task<bool> IsAuthorizedAsync(long userId);
        Task AuthorizeUserAsync(long userId);
    }
}