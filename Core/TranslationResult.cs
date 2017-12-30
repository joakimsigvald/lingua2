using System.Collections.Generic;

namespace Lingua.Core
{
    public class TranslationResult
    {
        public string Translation { get; set; }
        public IReason Reason { get; set; }
        public ITranslation[] Translations { get; set; }
        public IList<ITranslation[]> Possibilities { get; set; }
    }
}