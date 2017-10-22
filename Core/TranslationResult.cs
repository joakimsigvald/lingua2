using System.Collections.Generic;

namespace Lingua.Core
{
    public class TranslationResult
    {
        public string Translation { get; set; }
        public IReason Reason { get; set; }
        public IList<Translation[]> Candidates { get; set; }
        public Translation[] Translations { get; set; }
    }
}