using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Learning
{
    public class PatternGenerator
    {
        private readonly ITranslationExtractor _translationExtractor;
        private readonly IPatternExtractor _patternExtractor;

        public PatternGenerator(ITranslationExtractor translationExtractor, IPatternExtractor patternExtractor)
        {
            _translationExtractor = translationExtractor;
            _patternExtractor = patternExtractor;
        }

        public IList<ScoredPattern> GetScoredPatterns(TestCaseResult result)
        {
            var wanted = _translationExtractor.GetWantedTranslations(result).ToList();
            var unwanted = _translationExtractor.GetUnwantedTranslations(result).ToList();
            var monoCodes= _patternExtractor
                .GetMatchingMonoCodes(wanted)
                .Select(code => new ScoredPattern(code, 1, 1))
                .Concat(_patternExtractor.GetMatchingMonoCodes(unwanted)
                    .Select(code => new ScoredPattern(code, 1, -1)));
            var multiCodes =
                        Enumerable.Range(2, 5)
                            .SelectMany(length => GetMultiCodes(wanted, unwanted, (byte)length));
            return monoCodes.Concat(multiCodes).ToArray();
        }

        private IEnumerable<ScoredPattern> GetMultiCodes(
            ICollection<Translation> wanted
            , ICollection<Translation> unwanted
            , byte length)
            => _patternExtractor.GetMatchingCodes(wanted, length)
                .Select(code => new ScoredPattern(code, length, 1))
                .Concat(_patternExtractor.GetMatchingCodes(unwanted, length)
                    .Select(code => new ScoredPattern(code, length, -1)));
    }
}