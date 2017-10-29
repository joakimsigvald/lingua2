using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;

    public interface ITranslationExtractor
    {
        IEnumerable<Translation[]> GetWantedTranslations(TestCaseResult result);
        IEnumerable<Translation> GetUnwantedTranslations(TestCaseResult result);
    }

    public class TranslationExtractor : ITranslationExtractor
    {
        public IEnumerable<Translation[]> GetWantedTranslations(TestCaseResult result)
            => result.ExpectedCandidates;

        public IEnumerable<Translation> GetUnwantedTranslations(TestCaseResult result)
            => result.Translations;
    }
}