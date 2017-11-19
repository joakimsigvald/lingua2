using System.Collections.Generic;

namespace Lingua.Learning
{
    using Core;

    public interface ITranslationExtractor
    {
        IEnumerable<Translation> GetWantedTranslations(TestCaseResult result);
        IEnumerable<Translation> GetUnwantedTranslations(TestCaseResult result);
    }
}