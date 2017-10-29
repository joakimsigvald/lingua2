using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;

    public static class PatternGenerator
    {
        public static IList<(string, sbyte)> GetMatchingPatterns(TestCaseResult result)
        {
            var wantedAlternatives = GetWantedTranslations(result).ToList();
            var unwanted = GetUnwantedTranslations(result)
                .ToList();
            var unwantedAlternatives = GetUnwantedTranslations(result)
                .Select(t => new[] { t })
                .ToList();
            return PatternExtractor.GetMatchingMonoPatterns(wantedAlternatives.SelectMany(a => a).ToArray())
                .Select(x => (x, (sbyte)1))
                .Concat(PatternExtractor.GetMatchingMonoPatterns(unwanted)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(PatternExtractor.GetMatchingPatterns(wantedAlternatives, 2)
                    .Select(x => (x, (sbyte)1)))
                .Concat(PatternExtractor.GetMatchingPatterns(unwantedAlternatives, 2)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(PatternExtractor.GetMatchingPatterns(wantedAlternatives, 3)
                    .Select(x => (x, (sbyte)1)))
                .Concat(PatternExtractor.GetMatchingPatterns(unwantedAlternatives, 3)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(PatternExtractor.GetMatchingPatterns(wantedAlternatives, 4)
                    .Select(x => (x, (sbyte)1)))
                .Concat(PatternExtractor.GetMatchingPatterns(unwantedAlternatives, 4)
                    .Select(x => (x, (sbyte)-1)))
                    .ToList();
        }

        private static IEnumerable<Translation[]> GetWantedTranslations(TestCaseResult result)
            => result.ExpectedCandidates;

        private static IEnumerable<Translation> GetUnwantedTranslations(TestCaseResult result)
            => result.Translations;
    }
}