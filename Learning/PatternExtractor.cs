using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Tokens;

    public interface IPatternExtractor
    {
        IEnumerable<string> GetMatchingMonoPatterns(IEnumerable<Translation> translations);
        IEnumerable<string> GetMatchingPatterns(ICollection<Translation[]> candidates, int n);
    }

    public class PatternExtractor : IPatternExtractor
    {
        private static readonly List<IList<ushort[]>> Masks = new List<IList<ushort[]>>();

        public IEnumerable<string> GetMatchingMonoPatterns(IEnumerable<Translation> translations)
            => GetMatchingMonoCodes(translations)
                .Select(Encoder.Serialize);

        public IEnumerable<string> GetMatchingPatterns(ICollection<Translation[]> candidates, int n)
            => GetMatching(candidates, n)
                .Select(Encoder.Serialize);

        private static IEnumerable<ushort[]> GetMatchingMonoCodes(IEnumerable<Translation> translations)
        {
            var codes = Encoder.Encode(translations.Select(t => t.From)).ToArray();
            var generalizedCodes = codes.Select(code => Mask(Encoder.ModifiersMask, code)).Distinct().ToArray();
            return generalizedCodes
                .Concat(codes)
                .Distinct()
                .Select(code => new[] { code });
        }

        private static IEnumerable<ushort[]> GetMatching(ICollection<Translation[]> candidates, int n)
        {
            if (candidates.Count < n - 1)
                return new ushort[0][];

            var startingCodes = GetStartingCodes(candidates, n).ToList();
            var nonStartingCodes = GetNonStartingCodes(candidates, n).ToList();
            return startingCodes.Concat(nonStartingCodes);
        }

        private static IEnumerable<ushort[]> GetStartingCodes(IEnumerable<Translation[]> candidates, int n)
            => MaskSnippets(GetFirstSnippets(candidates, n - 1), n - 1)
                .Select(code => code.Prepend(Start.Code).ToArray());

        private static IEnumerable<ushort[]> GetNonStartingCodes(ICollection<Translation[]> candidates, int n)
            => MaskSnippets(GetSnippets(candidates, n), n)
                .Distinct();

        private static IEnumerable<ushort[]> GetSnippets(ICollection<Translation[]> candidates, int n)
            => Enumerable.Range(0, candidates.Count + 1 - n)
            .SelectMany(i => GetFirstSnippets(candidates.Skip(i), n));

        private static IEnumerable<ushort[]> GetFirstSnippets(IEnumerable<Translation[]> candidates, int n)
        {
            var tree = Combine(candidates);
            var paths = Expand(tree, n);
            return paths.Where(path => path.Length == n);
        }

        private static TranslationTreeNode Combine(IEnumerable<Translation[]> candidates)
            => new TranslationTreeNode(null, candidates.ToList());

        private static IEnumerable<ushort[]> Expand(TranslationTreeNode node, int n)
            => node.ExpandChildren(n).Select(l => l.Select(tn => Encoder.Encode(tn.Translation.From)).ToArray());

        private static IEnumerable<ushort[]> MaskSnippets(IEnumerable<ushort[]> snippets, int n)
            => GetMasks(n).SelectMany(
                mask => snippets.Select(code => ApplyMask(code, mask)));

        private static ushort[] ApplyMask(ushort[] code, ushort[] mask)
            => code.Select((c, i) => Mask(c, mask[i])).ToArray();

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

        private static ushort Mask(ushort code, ushort mask)
            => (ushort)(code | mask);
    }
}