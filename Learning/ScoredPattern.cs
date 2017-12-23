using System;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Extensions;

namespace Lingua.Learning
{
    public class ScoredPattern
    {
        public ScoredPattern(ushort[] code, sbyte score)
        {
            Code = code;
            Score = score;
            Priority = ComputePriority();
        }

        private int ComputePriority()
            => (int)(ComputeSize() * Math.Sqrt(Math.Abs(Score)) * (Score < 0 ? 2 : 1));

        private int ComputeSize()
            => Code.Sum(ComputeSize);

        private static int ComputeSize(ushort code)
        {
            var modifiers = code & Encoder.ProperModifiersMask;
            var bitCount = modifiers.CountBits();
            var size = 3 + (int)Math.Pow(2, bitCount);
            return (code & Encoder.Wildcard) == 0 ? size : size - 3;
        }

        public ushort[] Code{ get; }
        public string Pattern => Encoder.Serialize(Code);
        public sbyte Score { get; }
        public int Priority { get; }
    }
}