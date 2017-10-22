using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    using Core;

    public static class PatternGenerator
    {
        private static readonly CodeEqualityComparer CodeEqualityComparer = new CodeEqualityComparer();

        public static IEnumerable<string> GetMatchingPatterns(TestCaseResult result)
        {
            var atoms = YieldMatchingAtoms(result).Distinct().ToArray();
            return GetMatchingMonoCodes(atoms.SelectMany(a => a).ToArray())
                .Concat(GetMatchingTwins(atoms))
                .Select(Encoder.Serialize);
        }

        private static IEnumerable<ushort[]> GetMatchingMonoCodes(ushort[] atoms)
        {
            var generalizedCodes = atoms.Select(code => Mask(Encoder.ModifiersMask, code)).Distinct().ToArray();
            return generalizedCodes
                .Concat(atoms)
                .Select(code => new[] {code});
        }

        private static IEnumerable<ushort[]> GetMatchingTwins(IList<ushort[]> candidates)
        {
            var twins = candidates.Skip(1).SelectMany((c, i) => CreateTwins(candidates[i], c));
            return TwinMasks
                .SelectMany(
                    mask => twins.Select(twin => new[]
                    {
                        Mask(twin[0], mask[0]), Mask(twin[1], mask[1])
                    }))
                    .Distinct(CodeEqualityComparer);
        }

        private static IEnumerable<ushort[]> CreateTwins(ushort[] a, ushort[] b)
            => a.SelectMany(aa => b.Select(bb => new[] {aa, bb}));

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

        private static IEnumerable<ushort[]> YieldMatchingAtoms(TestCaseResult result)
            => result.ExpectedCandidates.Select(GetMatchingAtoms);

        private static ushort[] GetMatchingAtoms(IEnumerable<Translation> translations)
            => translations.Select(t => Encoder.Encode(t.From)).ToArray();
    }
}