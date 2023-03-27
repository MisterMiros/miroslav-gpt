using System.Threading.Tasks;

namespace MiroslavGPT.Domain.Interfaces
{
    public interface IUsersRepository
    {
        Task<bool> IsAuthorizedAsync(long userId);
        Task AuthorizeUserAsync(long userId);
        Task<bool> IsVoiceOverEnabledAsync(long userId);
        Task SetVoiceOverAsync(long userId, bool enabled);
    }
}