using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Extensions;
    using Grammar;

    public static class ArrangerGenerator
    {
        public static IEnumerable<Arranger> GetArrangerCandidates(IEnumerable<Arrangement> targetArrangements)
            => targetArrangements
                .SelectMany(GetArrangements)
                .Distinct()
                .OrderBy(arr => arr.Length)
                .Select(arr => new Arranger(arr));

        public static IEnumerable<Arranger> GetArrangerCandidates(Arrangement targetArrangement)
            => targetArrangement.IsInPerfectOrder 
            ? new Arranger[0]
            : GetArrangements(targetArrangement)
                .Distinct()
                .OrderBy(arr => arr.Length)
                .Select(arr => new Arranger(arr));

        private static IEnumerable<Arrangement> GetArrangements(Arrangement targetArrangement)
            => GetArrangements(targetArrangement.Code, targetArrangement.Order)
                .ExceptNull()
                .Where(arr => !arr.IsInPerfectOrder);

        private static IEnumerable<Arrangement?> GetArrangements(ushort[] code, byte[] order)
        {
            for (var length = 1; length <= code.Length; length++)
            for (var index = 0; index <= code.Length - length; index++)
                yield return GetArrangement(code, order, length, index);
        }

        private static Arrangement? GetArrangement(ushort[] code, byte[] order, int length, int startIindex)
        {
            var endIndex = startIindex + length;
            var suborder = order.Where(n => n >= startIindex && n < endIndex).ToArray();
            if (!suborder.IsSegmentOf(order))
                return null;
            var subCode = code.Skip(startIindex).Take(length).ToArray();
            return subCode.All(Encoder.IsElement)
                ? new Arrangement(
                    subCode,
                    suborder.Select(o => (byte)(o - startIindex)).ToArray())
                : null;
        }
    }
}