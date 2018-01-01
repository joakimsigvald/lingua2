using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Learning
{
    using Core.Extensions;
    using Grammar;

    public static class ArrangerGenerator
    {
        public static IEnumerable<Arrangement> GetArrangerCandidates(IEnumerable<Arrangement> targetArrangements)
            => targetArrangements
                .SelectMany(GetArrangerCandidates)
                .Distinct()
                .OrderBy(arr => arr.Length);

        private static IEnumerable<Arrangement> GetArrangerCandidates(Arrangement targetArrangement)
            => GetArrangerCandidates(targetArrangement.Code, targetArrangement.Order)
                .ExceptNull()
                .Where(arr => !arr.IsInPerfectOrder);

        private static IEnumerable<Arrangement> GetArrangerCandidates(ushort[] code, byte[] order)
        {
            for (var length = 1; length <= code.Length; length++)
            for (var index = 0; index <= code.Length - length; index++)
                yield return GetArrangerCandidate(code, order, length, index);
        }

        private static Arrangement GetArrangerCandidate(ushort[] code, byte[] order, int length, int startIindex)
        {
            var endIndex = startIindex + length;
            var suborder = order.Where(n => n >= startIindex && n < endIndex).ToArray();
            if (!suborder.IsSegmentOf(order))
                return null;
            var subCode = code.Skip(startIindex).Take(length).ToArray();
            if (!subCode.All(Encoder.IsElement))
                return null;
            return new Arrangement(
                subCode,
                suborder.Select(o => (byte) (o - startIindex)).ToArray());
        }
    }
}