using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiroslavGPT.Domain
{
    using System.Threading.Tasks;

    public interface ITranslator
    {
        Task<string> DetectLanguageAsync(string text);
        Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage);
    }
}
