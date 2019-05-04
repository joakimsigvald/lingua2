using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Extensions;
    using Grammar;

    public static class ArrangerGenerator
    {
        public static IEnumerable<Arrangement> GetArrangerCandidates(Arrangement targetArrangement)
            => targetArrangement.IsInPerfectOrder
            ? new Arrangement[0]
            : GetArrangements(targetArrangement)
                .Distinct()
                .OrderBy(arr => arr.Length)
                .ToArray();

        private static IEnumerable<Arrangement> GetArrangements(Arrangement targetArrangement)
            => GetArrangements(targetArrangement.Code, targetArrangement.Order)
                .ExceptNull()
                .Where(arr => !arr.IsInPerfectOrder);

        private static IEnumerable<Arrangement?> GetArrangements(ushort[] code, byte[] order)
            => Enumerable.Range(1, code.Length)
            .SelectMany(len => Enumerable.Range(0, code.Length - len + 1)
            .Select(i => GetArrangement(code, order, len, i)));

        private static Arrangement? GetArrangement(ushort[] code, byte[] order, int length, int start)
        {
            var (subcode, suborder) = ExtractSub(code, order, start, start + length);
            return IsProperArrangement(order, subcode, suborder)
                ? CreateArrangement(subcode, suborder, start)
                : null;
        }

        private static (ushort[] code, byte[] order) ExtractSub(ushort[] code, byte[] order, int start, int end) 
            => (code[start..end], order.Where(n => n >= start && n < end).ToArray());

        private static bool IsProperArrangement(byte[] order, ushort[] subcode, byte[] suborder) 
            => suborder.IsSegmentOf(order) && subcode.All(Encoder.IsElementOrSeparator);

        private static Arrangement CreateArrangement(ushort[] subcode, byte[] suborder, int start)
            => new Arrangement(subcode, CreateOrder(suborder, start));

        private static byte[] CreateOrder(byte[] suborder, int start)
            => suborder.Select(i => (byte)(i - start)).ToArray();
    }
}