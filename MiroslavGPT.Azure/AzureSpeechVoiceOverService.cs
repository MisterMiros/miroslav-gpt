using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using MiroslavGPT.Azure.Settings;
using MiroslavGPT.Domain.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace MiroslavGPT.Azure
{
    public class AzureSpeechVoiceOverService : IVoiceOverService
    {
        private readonly IAzureSpeechSettings _settings;

        public AzureSpeechVoiceOverService(IAzureSpeechSettings settings)
        {
            _settings = settings;
        }


        private class CustomAudioDataStream : PushAudioOutputStreamCallback
        {
            private readonly MemoryStream _memoryStream = new MemoryStream();

            public override uint Write(byte[] dataBuffer)
            {
                _memoryStream.Write(dataBuffer, 0, dataBuffer.Length);
                return (uint)dataBuffer.Length;
            }

            public MemoryStream GetMemoryStream()
            {
                _memoryStream.Position = 0;
                return _memoryStream;
            }
        }

        public async Task<MemoryStream> VoiceOverAsync(string text)
        {
            var config = SpeechConfig.FromSubscription(_settings.AzureSpeechKey, _settings.AzureSpeechRegion);

            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz128KBitRateMonoMp3);
            config.SetProfanity(ProfanityOption.Raw);
            config.SpeechSynthesisVoiceName = "en-US-TonyNeural";

            var callback = new CustomAudioDataStream();
            var completed = false;
            using (var pushStream = AudioOutputStream.CreatePushStream(callback))
            {
                using var audioConfig = AudioConfig.FromStreamOutput(pushStream);
                using var synthesizer = new SpeechSynthesizer(config, audioConfig);
                {
                    using var result = await synthesizer.SpeakTextAsync(text);
                    {

                        completed = result.Reason == ResultReason.SynthesizingAudioCompleted;
                    }
                }
            }
            if (completed)
            {
                return callback.GetMemoryStream();
            }
            return null;
        }
    }
}
