using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core.Extensions;
    using Grammar;

    public static class ArrangerGenerator
    {
        public static IEnumerable<Arrangement> GetArrangerCandidates(IEnumerable<Arrangement> targetArrangements)
            => targetArrangements.SelectMany(arranger => GetArrangerCandidates(arranger.Code, arranger.Order))
                .NotNull()
                .Distinct()
                .Where(arr => !arr.IsInOrder)
                .OrderBy(arr => arr.Length);

        private static IEnumerable<Arrangement> GetArrangerCandidates(ushort[] code, byte[] order)
        {
            for (var l = 1; l <= code.Length; l++)
            for (var i = 0; i <= code.Length - l; i++)
                yield return GetArrangerCandidate(code, order, l, i);
        }

        private static Arrangement GetArrangerCandidate(ushort[] code, byte[] order, int l, int i)
        {
            var suborder = order.Where(n => n >= i && n < i + l).ToArray();
            if (!suborder.IsSegmentOf(order))
                return null;
            return new Arrangement(
                code.Skip(i).Take(l).ToArray(),
                suborder.Select(o => (byte) (o - i)).ToArray());
        }
    }
}