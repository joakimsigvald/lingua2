using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Extensions;

namespace Lingua.Learning
{
    using Core;
    using Core.Tokens;

    public interface IPatternExtractor
    {
        IEnumerable<string> GetMatchingMonoPatterns(IEnumerable<Translation> translations);
        IEnumerable<string> GetMatchingPatterns(ICollection<Translation> sequence, int length);
    }

    public class PatternExtractor : IPatternExtractor
    {
        private static readonly List<IList<ushort[]>> Masks = new List<IList<ushort[]>>();

        public IEnumerable<string> GetMatchingMonoPatterns(IEnumerable<Translation> translations)
            => GetMatchingMonoCodes(translations)
                .Select(Encoder.Serialize);

        public IEnumerable<string> GetMatchingPatterns(ICollection<Translation> sequence, int length)
            => GetMatchingSnippets(sequence, length)
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
        private static IEnumerable<ushort[]> GetMatchingSnippets(ICollection<Translation> sequence, int length)
        {
            if (sequence.Count < length - 1)
                return new ushort[0][];

            var startingCodes = GetStartingCodes(sequence, length - 1).ToList();
            if (sequence.Count < length - 1)
                return startingCodes;

            var nonStartingCodes = GetNonStartingCodes(sequence, length).ToList();
            return startingCodes.Concat(nonStartingCodes);
        }

        private static IEnumerable<ushort[]> GetStartingCodes(IEnumerable<Translation> sequence, int length)
            => MaskSnippet(Encode(sequence.Take(length)))
                .Select(code => code.Prepend(Start.Code).ToArray());

        private static IEnumerable<ushort[]> GetNonStartingCodes(ICollection<Translation> sequence, int length)
            => MaskSnippets(GetSnippets(sequence, length), length).Distinct();

        private static IEnumerable<ushort[]> GetSnippets(ICollection<Translation> sequence, int length)
            => Enumerable.Range(0, sequence.Count + 1 - length)
                .Select(i => Encode(sequence.Skip(i).Take(length)));

        private static ushort[] Encode(IEnumerable<Translation> translations)
            => translations.Select(translation => Encoder.Encode(translation.From)).ToArray();

        private static IEnumerable<ushort[]> MaskSnippet(ushort[] snippet)
            => Mask(GetMasks(snippet.Length), snippet);

        private static IEnumerable<ushort[]> MaskSnippets(IEnumerable<ushort[]> snippets, int length)
            => GetMasks(length).SelectMany(mask => Mask(snippets, mask));

        private static IEnumerable<ushort[]> Mask(IEnumerable<ushort[]> snippets, ushort[] mask)
            => snippets.Select(snippet => ApplyMask(snippet, mask));

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