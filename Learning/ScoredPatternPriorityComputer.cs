using System;
using System.Linq;
using Lingua.Core.Tokens;

namespace Lingua.Learning
{
    using Core;
    using Core.Extensions;

    public static class ScoredPatternPriorityComputer
    {
        public static int ComputePriority(int currentScore, sbyte increment, ushort[] reversedCode)
            => (int)(
            (increment < 0 ? 2 : 1) 
            * ComputeScoreFactor(currentScore, increment) 
            * ComputeSize(reversedCode));

        private static double ComputeScoreFactor(int currentScore, sbyte increment)
            => currentScore == 0
                    ? 1
                    : increment * currentScore < 0
                        ? 0.25 * Math.Sqrt(Math.Abs(currentScore))
                        : Math.Sqrt(Math.Abs(currentScore) + 1);

        private static int ComputeSize(ushort[] code)
            => code.Sum(ComputeSize);

        private static int ComputeSize(ushort code)
        {
            if (code == AnyToken.Code)
                return 1;
            var modifiers = code & Encoder.ProperModifiersMask;
            var bitCount = modifiers.CountBits();
            var size = 3 + (int)Math.Pow(2, bitCount);
            return (code & Encoder.Wildcard) == 0 ? size : size - 2;
        }
    }
}