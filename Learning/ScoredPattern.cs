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

        private double ComputePriority()
            => ComputeSize() * Math.Sqrt(Math.Abs(Score)) * (Score < 0 ? 2 : 1);

        private int ComputeSize()
            => Code.Sum(ComputeSize);

        private static int ComputeSize(ushort code)
        {
            var modifiers = code & Encoder.ProperModifiersMask;
            var size = 2 + (int)Math.Pow(2, modifiers.CountBits());
            return (code & Encoder.Wildcard) == 0 ? size : size - 2;
        }

        public ushort[] Code{ get; }
        public string Pattern => Encoder.Serialize(Code);
        public sbyte Score { get; }
        public double Priority { get; }
    }
}