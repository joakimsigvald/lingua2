using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    using Core;

    public static class PatternGenerator
    {
        public static IEnumerable<(string, sbyte)> GetMatchingPatterns(TestCaseResult result)
        {
            var wanted = GetWantedAtoms(result).ToArray();
            var unwanted = GetUnwantedAtoms(result).ToArray();
            return PatternExtractor.GetMatchingMonoPatterns(wanted.SelectMany(a => a).ToArray())
                .Select(x => (x, (sbyte)1))
                .Concat(PatternExtractor.GetMatchingMonoPatterns(unwanted)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(PatternExtractor.GetMatchingPatterns(wanted, 2)
                    .Select(x => (x, (sbyte)1)))
                .Concat(PatternExtractor.GetMatchingPatterns(unwanted, 2)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(PatternExtractor.GetMatchingPatterns(wanted, 3)
                    .Select(x => (x, (sbyte)1)))
                .Concat(PatternExtractor.GetMatchingPatterns(unwanted, 3)
                    .Select(x => (x, (sbyte)-1)))
                .Concat(PatternExtractor.GetMatchingPatterns(wanted, 4)
                    .Select(x => (x, (sbyte)1)))
                .Concat(PatternExtractor.GetMatchingPatterns(unwanted, 4)
                    .Select(x => (x, (sbyte)-1)));
        }

        private static IEnumerable<ushort[]> GetWantedAtoms(TestCaseResult result)
            => result.ExpectedCandidates.Select(Encode);

        private static IEnumerable<ushort> GetUnwantedAtoms(TestCaseResult result)
            => Encode(result.Translations);

        private static ushort[] Encode(IEnumerable<Translation> translations)
            => translations.Select(t => Encoder.Encode(t.From)).ToArray();
    }
}