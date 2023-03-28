using MiroslavGPT.Domain.Interfaces;

namespace MiroslavGPT.AWS
{
    public class EmptyVoiceOverService : IVoiceOverService
    {
        public Task<MemoryStream> VoiceOverAsync(string text)
        {
            return Task.FromResult(null);
        }
    }
}
