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

        public IList<(string, sbyte)> GetMatchingPatterns(TestCaseResult result)
        {
            var wantedAlternatives = _translationExtractor.GetWantedTranslations(result).ToList();
            var unwanted = _translationExtractor.GetUnwantedTranslations(result).ToList();
            var monopatterns = _patternExtractor
                .GetMatchingMonoPatterns(wantedAlternatives.SelectMany(a => a).ToArray())
                .Select(pattern => (pattern, (sbyte) 1))
                .Concat(_patternExtractor.GetMatchingMonoPatterns(unwanted)
                    .Select(pattern => (pattern, (sbyte) -1)));
            var multipatterns =
                Enumerable.Range(1, 2)
                    .SelectMany(score =>
                        Enumerable.Range(2, 3)
                            .SelectMany(length => GetMultiPatterns(wantedAlternatives, unwanted, length, score)));
            var allPAtterns = monopatterns.Concat(multipatterns).ToArray();
            return allPAtterns.ToList();
        }

        private IEnumerable<(string, sbyte)> GetMultiPatterns(
            ICollection<Translation[]> wantedAlternatives
            , ICollection<Translation> unwanted
            , int length
            , int score)
            => _patternExtractor.GetMatchingPatterns(wantedAlternatives, length)
                .Select(pattern => (pattern, (sbyte) score))
                .Concat(_patternExtractor.GetMatchingPatterns(unwanted, length)
                    .Select(pattern => (pattern, (sbyte) -score)));
    }
}