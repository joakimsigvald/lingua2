using System;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Extensions;

namespace Lingua.Learning
{
    public static class ScoredPatternPriorityComputer
    {
        public static int ComputePriority(int score, ushort[] code)
            => (int)(ComputeSize(code) * Math.Sqrt(Math.Abs(score)) * (score < 0 ? 2 : 1));

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