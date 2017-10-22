using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    using Core;
    using Core.Tokens;

    public static class PatternGenerator
    {
        public static IEnumerable<(string, sbyte)> GetMatchingPatterns(TestCaseResult result)
        {
            var wanted = GetWantedAtoms(result).ToArray();
            var unwanted = GetUnwantedAtoms(result).ToArray();
            return GetMatchingMonoCodes(wanted.SelectMany(a => a).ToArray())
                .Select(x => (Encoder.Serialize(x), (sbyte) 1))
                .Concat(GetMatchingMonoCodes(unwanted)
                    .Select(x => (Encoder.Serialize(x), (sbyte) -1)))
                .Concat(GetMatchingTwins(wanted)
                    .Select(x => (Encoder.Serialize(x), (sbyte) 1)))
                .Concat(GetMatchingTwins(unwanted)
                    .Select(x => (Encoder.Serialize(x), (sbyte) -1)));
        }

        private static IEnumerable<ushort[]> GetWantedAtoms(TestCaseResult result)
            => result.ExpectedCandidates.Select(Encode);

        private static IEnumerable<ushort> GetUnwantedAtoms(TestCaseResult result)
            => Encode(result.Translations);

        private static ushort[] Encode(IEnumerable<Translation> translations)
            => translations.Select(t => Encoder.Encode(t.From)).ToArray();

        private static IEnumerable<ushort[]> GetMatchingMonoCodes(ushort[] atoms)
        {
            var generalizedCodes = atoms.Select(code => Mask(Encoder.ModifiersMask, code)).Distinct().ToArray();
            return generalizedCodes
                .Concat(atoms)
                .Distinct()
                .Select(code => new[] {code});
        }

        private static IEnumerable<ushort[]> GetMatchingTwins(IList<ushort[]> candidates)
            => MaskTwins(candidates
                .Prepend(new[] { Start.Code })
                .Take(candidates.Count)
                .SelectMany((c, i) => CreateTwins(c, candidates[i])))
                .Distinct(CodeEqualityComparer.Singleton);

        private static IEnumerable<ushort[]> CreateTwins(ushort[] a, ushort[] b)
            => a.SelectMany(aa => b.Select(bb => new[] { aa, bb }));

        private static IEnumerable<ushort[]> GetMatchingTwins(IList<ushort> codes) 
            => MaskTwins(codes
            .Prepend(Start.Code)
            .Take(codes.Count)
            .Select((c, i) => new[] { c, codes[i] }))
            .Distinct(CodeEqualityComparer.Singleton);

        private static IEnumerable<ushort[]> MaskTwins(IEnumerable<ushort[]> twins)
            => TwinMasks
                .SelectMany(
                    mask => twins.Select(twin => new[]
                    {
                        Mask(twin[0], mask[0]), Mask(twin[1], mask[1])
                    }));

        private static readonly IList<ushort[]> TwinMasks =
            new[]
            {
                new[] {Encoder.ModifiersMask, Encoder.ModifiersMask},
                new ushort[] {0, Encoder.ModifiersMask},
                new ushort[] {Encoder.ModifiersMask, 0},
                new ushort[] {0, 0}
            };

        private static ushort Mask(ushort code, ushort mask)
            => (ushort) (code | mask);
    }
}