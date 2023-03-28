namespace MiroslavGPT.Domain.Interfaces
{
    public interface IVoiceOverService
    {
        public Task<MemoryStream> VoiceOverAsync(string text);
    }
}
