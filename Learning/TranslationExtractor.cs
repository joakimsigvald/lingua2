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

        private static IEnumerable<ITranslation> GetWantedTranslations(TestCaseResult result)
            => result?.ExpectedTranslations?? Enumerable.Empty<ITranslation>();

        private static IEnumerable<ITranslation> GetUnwantedTranslations(TestCaseResult result)
            => result?.Reduction.Translations ?? new ITranslation[0];
    }
}