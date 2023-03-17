using System.Threading.Tasks;

namespace MiroslavGPT.Domain
{
    public interface IUsersRepository
    {
        Task<bool> IsAuthorizedAsync(long userId);
        Task AuthorizeUserAsync(long userId);
    }
}