using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;

    public class TranslationExtractor : ITranslationExtractor
    {
        public IEnumerable<Translation[]> GetWantedTranslations(TestCaseResult result)
            => result?.ExpectedCandidates ?? Enumerable.Empty<Translation[]>();

        public IEnumerable<Translation> GetUnwantedTranslations(TestCaseResult result)
            => result?.Translations ?? Enumerable.Empty<Translation>();
    }
}