using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    public class PatternGenerator
    {
        private readonly IPatternExtractor _patternExtractor;
        private readonly ITranslationExtractor _translationExtractor;

        public PatternGenerator(IPatternExtractor patternExtractor, ITranslationExtractor translationExtractor)
        {
            _patternExtractor = patternExtractor;
            _translationExtractor = translationExtractor;
        }

        public IList<(string, sbyte)> GetMatchingPatterns(TestCaseResult result)
        {
            var wantedAlternatives = _translationExtractor.GetWantedTranslations(result).ToList();
            var unwanted = _translationExtractor.GetUnwantedTranslations(result)
                .ToList();
            var unwantedAlternatives = unwanted
                .Select(t => new[] { t })
                .ToList();
            return _patternExtractor.GetMatchingMonoPatterns(wantedAlternatives.SelectMany(a => a).ToArray())
                .Select(x => (x, (sbyte)1))
                .Concat(_patternExtractor.GetMatchingMonoPatterns(unwanted)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(_patternExtractor.GetMatchingPatterns(wantedAlternatives, 2)
                    .Select(x => (x, (sbyte)1)))
                .Concat(_patternExtractor.GetMatchingPatterns(unwantedAlternatives, 2)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(_patternExtractor.GetMatchingPatterns(wantedAlternatives, 3)
                    .Select(x => (x, (sbyte)1)))
                .Concat(_patternExtractor.GetMatchingPatterns(unwantedAlternatives, 3)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(_patternExtractor.GetMatchingPatterns(wantedAlternatives, 4)
                    .Select(x => (x, (sbyte)1)))
                .Concat(_patternExtractor.GetMatchingPatterns(unwantedAlternatives, 4)
                    .Select(x => (x, (sbyte)-1)))
                    .ToList();
        }
    }
}