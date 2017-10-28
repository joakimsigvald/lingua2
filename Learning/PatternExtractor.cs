using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    using Core;
    using Core.Tokens;

    public static class PatternExtractor
    {
        public static IEnumerable<string> GetMatchingMonoPatterns(ushort[] atoms)
            => GetMatchingMonoCodes(atoms)
            .Select(Encoder.Serialize);

        public static IEnumerable<string> GetMatchingPatterns(ICollection<ushort> candidates, int n)
            => GetMatchingCodes(candidates, n)
            .Select(Encoder.Serialize);

        public static IEnumerable<string> GetMatchingPatterns(ICollection<ushort[]> candidates, int n)
            => GetMatching(candidates, n)
                .Select(Encoder.Serialize);

        private static IEnumerable<ushort[]> GetMatchingMonoCodes(ushort[] atoms)
        {
            var generalizedCodes = atoms.Select(code => Mask(Encoder.ModifiersMask, code)).Distinct().ToArray();
            return generalizedCodes
                .Concat(atoms)
                .Distinct()
                .Select(code => new[] { code });
        }

        private static IEnumerable<ushort[]> GetMatchingCodes(ICollection<ushort> candidates, int n)
        {
            if (candidates.Count < n - 1)
                return new ushort[0][];
            var startingCodes = GetStartingCodes(candidates, n);
            var nonStartingCodes = GetNonStartingCodes(candidates, n);
            return startingCodes.Concat(nonStartingCodes);
        }

        private static IEnumerable<ushort[]> GetStartingCodes(IEnumerable<ushort> candidates, int n)
        {
            var startingSnippet = candidates.Take(n - 1).ToArray();
            return GetMasks(n - 1).Select(
                mask => ApplyMask(mask, startingSnippet))
                .Select(code => code.Prepend(Start.Code).ToArray());
        }

        private static IEnumerable<ushort[]> GetNonStartingCodes(ICollection<ushort> candidates, int n)
        {
            var snippets = Enumerable.Range(0, candidates.Count + 1 - n)
                .Select(i => candidates.Skip(i).Take(n).ToArray());
            return MaskSnippets(snippets, n);
        }

        private static IEnumerable<ushort[]> GetMatching(ICollection<ushort[]> candidates, int n)
        {
            if (candidates.Count < n - 1)
                return new ushort[0][];
            var startingCodes = GetStartingCodes(candidates, n);
            var nonStartingCodes = GetNonStartingCodes(candidates, n);
            return startingCodes.Concat(nonStartingCodes);
        }

        private static IEnumerable<ushort[]> GetStartingCodes(IEnumerable<ushort[]> candidates, int n)
        {
            var startingCandidates = candidates.Take(n - 1).ToArray();
            var startingSnippets = GetCombinations(startingCandidates).ToList();
            return MaskSnippets(startingSnippets, n - 1)
                .Select(code => code.Prepend(Start.Code).ToArray());
        }

        private static IEnumerable<ushort[]> GetNonStartingCodes(ICollection<ushort[]> candidates, int n)
        {
            var snippetCount = candidates.Count + 1 - n;
            var candidateSnippets = Enumerable.Range(0, snippetCount)
                .Select(i => candidates.Skip(i).Take(n).ToList());
            var snippets = GetCombinations(candidateSnippets).ToList();
            return MaskSnippets(snippets, n)
                .Distinct();
        }

        private static IEnumerable<ushort[]> MaskSnippets(IEnumerable<ushort[]> snippets, int n)
        {
            var masks = GetMasks(n);
            return masks.SelectMany(
                mask => snippets.Select(code => ApplyMask(code, mask)));
        }

        private static ushort[] ApplyMask(ushort[] code, ushort[] mask)
            => code.Select((c, i) => Mask(c, mask[i])).ToArray();

        private static IEnumerable<ushort[]> GetCombinations(IEnumerable<List<ushort[]>> candidateSnippets)
            => candidateSnippets.SelectMany(GetCombinations);

        private static IEnumerable<ushort[]> GetCombinations(IList<ushort[]> candidateSnippet)
            => candidateSnippet.Any()
                ? candidateSnippet.First()
                    .SelectMany(code => GetCombinations(candidateSnippet.Skip(1).ToList())
                        .Select(sequence => sequence.Prepend(code).ToArray()))
                : new[] { new ushort[0] };

        private static IEnumerable<ushort[]> GetMasks(int n)
        {
            var i = n - 1;
            while (i >= Masks.Count)
                Masks.Add(null);
            return Masks[i] ?? (Masks[i] = CreateMasks(n).ToArray());
        }

        private static IEnumerable<ushort[]> CreateMasks(int n)
            => Enumerable.Range(0, 1 << n)
                .Select(i => CreateMask(i, n).ToArray());

        private static IEnumerable<ushort> CreateMask(int i, int n)
            => Enumerable.Range(0, n)
                .Select(p => (i >> p) % 2 == 0 ? Encoder.ModifiersMask : (ushort)0);

        private static readonly List<IList<ushort[]>> Masks = new List<IList<ushort[]>>();

        private static ushort Mask(ushort code, ushort mask)
            => (ushort)(code | mask);
    }
}