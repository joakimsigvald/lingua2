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

        public EvaluatorEnhancement GetEvaluatorEnhancements(TestCaseResult result)
        {
            var wanted = _translationExtractor.GetWantedTranslations(result).ToList();
            var unwanted = _translationExtractor.GetUnwantedTranslations(result).ToList();
            var monopatterns = _patternExtractor
                .GetMatchingMonoPatterns(wanted)
                .Select(pattern => new ScoredPattern(pattern, 1))
                .Concat(_patternExtractor.GetMatchingMonoPatterns(unwanted)
                    .Select(pattern => new ScoredPattern(pattern, -1)));
            var multipatterns =
                        Enumerable.Range(2, 3)
                            .SelectMany(length => GetMultiPatterns(wanted, unwanted, length));
            return new EvaluatorEnhancement(monopatterns.Concat(multipatterns).ToArray());
        }

        private IEnumerable<ScoredPattern> GetMultiPatterns(
            ICollection<Translation> wanted
            , ICollection<Translation> unwanted
            , int length)
            => _patternExtractor.GetMatchingPatterns(wanted, length)
                .Select(pattern => new ScoredPattern(pattern, 1))
                .Concat(_patternExtractor.GetMatchingPatterns(unwanted, length)
                    .Select(pattern => new ScoredPattern(pattern, -1)));
    }
}