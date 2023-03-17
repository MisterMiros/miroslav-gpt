using System.Threading.Tasks;
using Amazon;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using Amazon.Translate;
using Amazon.Translate.Model;
using MiroslavGPT.Domain;

namespace MiroslavGPT.AWS
{
    public class AmazonTranslator : ITranslator
    {
        private readonly AmazonTranslateClient _translateClient;
        private readonly AmazonComprehendClient _comprehendClient;

        public AmazonTranslator(string region)
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(region);
            _translateClient = new AmazonTranslateClient(regionEndpoint);
            _comprehendClient = new AmazonComprehendClient(regionEndpoint);
        }

        public async Task<string> TranslateTextAsync(string sourceLanguage, string targetLanguage, string text)
        {
            var request = new TranslateTextRequest
            {
                SourceLanguageCode = sourceLanguage,
                TargetLanguageCode = targetLanguage,
                Text = text
            };

            TranslateTextResponse response = await _translateClient.TranslateTextAsync(request);
            return response.TranslatedText;
        }

        public async Task<string> DetectLanguageAsync(string text)
        {
            var request = new DetectDominantLanguageRequest
            {
                Text = text
            };

            DetectDominantLanguageResponse response = await _comprehendClient.DetectDominantLanguageAsync(request);
            string dominantLanguage = response.Languages[0].LanguageCode;
            return dominantLanguage;
        }
    }
}
