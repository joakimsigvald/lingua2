using System.Collections.Generic;

namespace Lingua.Core
{
    public class TranslationResult
    {
        public TranslationResult(string translation)
            => Translation = translation;

        public string Translation { get; }
        public IReason Reason { get; set; }
        public ITranslation[] Translations { get; set; }
        public IList<ITranslation[]> Possibilities { get; set; }
    }
}