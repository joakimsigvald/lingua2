using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Extensions;

namespace Lingua.Learning
{
    using Core;
    using Core.Tokens;

    public class PatternExtractor : IPatternExtractor
    {
        private static readonly List<IList<bool[]>> Masks = new List<IList<bool[]>>();

        public IEnumerable<ushort[]> GetMatchingMonoCodes(ushort[] sequence)
        {
            var codes = sequence.Distinct().ToArray();
            return codes.SelectMany(Encoder.Generalize)
                .Concat(codes)
                .Select(code => new[] { code });
        }

        public IEnumerable<ushort[]> GetMatchingCodes(ushort[] sequence, int length)
        {
            if (sequence.Length < length - 1)
                return new ushort[0][];

            var startingCodes = GetStartingCodes(sequence, length - 1);
            if (sequence.Length < length)
                return startingCodes;

            var nonStartingCodes = GetNonStartingCodes(sequence, length);
            return startingCodes.Concat(nonStartingCodes);
        }

        private static IEnumerable<ushort[]> GetStartingCodes(ushort[] sequence, int length)
            => MaskSnippet(sequence.Take(length).ToArray())
                .Select(code => code.Prepend(Start.Code).ToArray());

        private static IEnumerable<ushort[]> GetNonStartingCodes(ushort[] sequence, int length)
            => MaskSnippets(GetSnippets(sequence, length), length).Distinct();

        private static IEnumerable<ushort[]> GetSnippets(ushort[] sequence, int length)
            => Enumerable.Range(0, sequence.Length + 1 - length)
                .Select(i => sequence.Skip(i).Take(length).ToArray());

        private static IEnumerable<ushort[]> MaskSnippet(ushort[] snippet)
            => GetMasks(snippet.Length).SelectMany(mask => ApplyMask(snippet, mask));

        private static IEnumerable<ushort[]> MaskSnippets(IEnumerable<ushort[]> snippets, int length)
            => GetMasks(length).SelectMany(mask => Mask(snippets, mask));

        private static IEnumerable<ushort[]> Mask(IEnumerable<ushort[]> snippets, bool[] mask)
            => snippets.SelectMany(snippet => ApplyMask(snippet, mask));

        private static IEnumerable<ushort[]> ApplyMask(ushort[] code, bool[] mask)
            => code.Select((c, i) => Mask(c, mask[i])).Combine();

        private static IEnumerable<bool[]> GetMasks(int n)
        {
            var i = n - 1;
            while (i >= Masks.Count)
                Masks.Add(null);
            return Masks[i] ?? (Masks[i] = CreateMasks(n).ToArray());
        }

        private static IEnumerable<bool[]> CreateMasks(int n)
            => Enumerable.Range(0, 1 << n)
                .Select(i => CreateMask(i, n).ToArray());

        private static IEnumerable<bool> CreateMask(int i, int n)
            => Enumerable.Range(0, n)
                .Select(p => (i >> p) % 2 == 0);

        private static IEnumerable<ushort> Mask(ushort code, bool mask)
            => mask ? Encoder.Generalize(code) : new []{code};
    }
}