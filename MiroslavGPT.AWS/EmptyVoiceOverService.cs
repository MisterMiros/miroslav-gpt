using MiroslavGPT.Domain.Interfaces;

namespace MiroslavGPT.AWS
{
    public class EmptyVoiceOverService : IVoiceOverService
    {
        public MemoryStream VoiceOver(string text)
        {
            return null;
        }
    }
}
