using System;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Extensions;

    public static class ScoredPatternPriorityComputer
    {
        public static int ComputePriority(int currentScore, sbyte increment, ushort[] code)
        {
            var scoreFactor =
                currentScore == 0
                    ? 1
                    : increment * currentScore < 0
                        ? 0.25 * Math.Sqrt(Math.Abs(currentScore))
                        : Math.Sqrt(Math.Abs(currentScore) + 1);
            if (increment < 0) scoreFactor *= 2;
            return (int) (scoreFactor * ComputeSize(code));
        }

        private static int ComputeSize(ushort[] code)
            => code.Sum(ComputeSize);

        private static int ComputeSize(ushort code)
        {
            var modifiers = code & Encoder.ProperModifiersMask;
            var bitCount = modifiers.CountBits();
            var size = 3 + (int)Math.Pow(2, bitCount);
            return (code & Encoder.Wildcard) == 0 ? size : size - 3;
        }
    }
}