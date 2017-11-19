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
            var wanted = _translationExtractor.GetWantedTranslations(result).ToList();
            var unwanted = _translationExtractor.GetUnwantedTranslations(result).ToList();
            var monopatterns = _patternExtractor
                .GetMatchingMonoPatterns(wanted)
                .Select(pattern => (pattern, (sbyte) 1))
                .Concat(_patternExtractor.GetMatchingMonoPatterns(unwanted)
                    .Select(pattern => (pattern, (sbyte) -1)));
            var multipatterns =
                        Enumerable.Range(2, 3)
                            .SelectMany(length => GetMultiPatterns(wanted, unwanted, length));
            var allPAtterns = monopatterns.Concat(multipatterns).ToArray();
            return allPAtterns.ToList();
        }

        private IEnumerable<(string, sbyte)> GetMultiPatterns(
            ICollection<Translation> wanted
            , ICollection<Translation> unwanted
            , int length
            , int score = 1)
            => _patternExtractor.GetMatchingPatterns(wanted, length)
                .Select(pattern => (pattern, (sbyte) score))
                .Concat(_patternExtractor.GetMatchingPatterns(unwanted, length)
                    .Select(pattern => (pattern, (sbyte) -score)));
    }
}