using Lingua.Core;
using Lingua.Core.Extensions;
using Lingua.Core.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    public class PatternExtractor : INewPatternExtractor
    {
        private static readonly List<IList<bool[]>> Masks = new List<IList<bool[]>>();

        public IEnumerable<Code> GetMatchingPatterns(Code code)
            => Generalize(code)
            .Where(c => !StartOrEndWithAnyToken(c))
            .Append(code);

        public bool StartOrEndWithAnyToken(Code code)
            => code.First() == AnyToken.Code || code.Last() == AnyToken.Code;

        private IEnumerable<Code> Generalize(Code code)
            => code.First() == Start.Code
            ? MaskSnippet(code.SkipFirst()).Select(rc => new Code(rc.Append(Start.Code))) 
            : MaskSnippet(code.ReversedCode).Select(rc => new Code(rc));

        private static IEnumerable<ushort[]> MaskSnippet(ushort[] snippet)
            => GetMasks(snippet.Length).SelectMany(mask => ApplyMask(snippet, mask));

        private static IEnumerable<ushort[]> ApplyMask(ushort[] code, bool[] mask)
            => code.Select((c, i) => Mask(c, mask[i])).Expand();

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
            => mask ? Encoder.Generalize(code) : new[] { code };
    }
}
