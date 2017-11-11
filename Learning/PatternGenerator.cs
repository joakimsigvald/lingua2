using System.Collections.Generic;
using System.Linq;

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

        public IList<(string, sbyte)> GetMatchingPatterns(TestCaseResult result)
        {
            var wantedAlternatives = _translationExtractor.GetWantedTranslations(result).ToList();
            var unwanted = _translationExtractor.GetUnwantedTranslations(result).ToList();
            return _patternExtractor.GetMatchingMonoPatterns(wantedAlternatives.SelectMany(a => a).ToArray())
                .Select(x => (x, (sbyte)1))
                .Concat(_patternExtractor.GetMatchingMonoPatterns(unwanted)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(_patternExtractor.GetMatchingPatterns(wantedAlternatives, 2)
                    .Select(x => (x, (sbyte)1)))
                .Concat(_patternExtractor.GetMatchingPatterns(unwanted, 2)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(_patternExtractor.GetMatchingPatterns(wantedAlternatives, 3)
                    .Select(x => (x, (sbyte)1)))
                .Concat(_patternExtractor.GetMatchingPatterns(unwanted, 3)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(_patternExtractor.GetMatchingPatterns(wantedAlternatives, 4)
                    .Select(x => (x, (sbyte)1)))
                .Concat(_patternExtractor.GetMatchingPatterns(unwanted, 4)
                    .Select(x => (x, (sbyte)-1)))
                    .ToList();
        }
    }
}