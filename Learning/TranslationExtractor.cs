using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;

    public class TranslationExtractor : ITranslationExtractor
    {
        public ushort[] GetWantedSequence(TestCaseResult result)
            => Encoder.Encode(GetWantedTranslations(result));

        public ushort[] GetUnwantedSequence(TestCaseResult result)
            => Encoder.Encode(GetUnwantedTranslations(result));

        private static IEnumerable<Translation> GetWantedTranslations(TestCaseResult result)
            => result?.ExpectedTranslations?? Enumerable.Empty<Translation>();

        private static IEnumerable<Translation> GetUnwantedTranslations(TestCaseResult result)
            => result?.Translations ?? Enumerable.Empty<Translation>();
    }
}