using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Grammar;

    public static class ArrangerGenerator
    {
        public static IEnumerable<Arranger> GetArrangerCandidates((ushort[] code, byte[] order)[] rearrangedTargets)
            => rearrangedTargets.SelectMany(tuple => GetArrangerCandidates(tuple.code, tuple.order))
                .OrderBy(arr => arr.Length);

        private static IEnumerable<Arranger> GetArrangerCandidates(ushort[] code, byte[] order)
        {
            for (var l = 1; l <= code.Length; l++)
            for (var i = 0; i <= code.Length - l; i++)
                yield return new Arranger(
                    code.Skip(i).Take(l).ToArray(),
                    order.Where(o => o > i && o <= l + i)
                        .Select(o => (byte)(o - i)).ToArray());
        }
    }
}