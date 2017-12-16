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
            Size = ComputeSize(code, score);
            Score = score;
        }

        private static byte ComputeSize(ushort[] code, sbyte score)
            => (byte)code.Sum(c => ComputeSize(c, score));

        private static byte ComputeSize(ushort code, sbyte score)
        {
            var modifiers = code & Encoder.ModifiersMask;
            var size = 3 - score + modifiers.CountBits();
            return (byte)(modifiers < Encoder.Wildcard ? size : size - 2);
        }

        public ushort[] Code{ get; }
        public string Pattern => Encoder.Serialize(Code);
        public sbyte Score { get; }
        public byte Size { get; }
    }
}