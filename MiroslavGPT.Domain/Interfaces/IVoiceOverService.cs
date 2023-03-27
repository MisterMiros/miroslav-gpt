namespace MiroslavGPT.Domain.Interfaces
{
    public interface IVoiceOverService
    {
        public MemoryStream VoiceOver(string text);
    }
}
