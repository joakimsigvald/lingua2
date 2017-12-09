using Lingua.Core;

namespace Lingua.Learning
{
    public class ScoredPattern
    {
        public ScoredPattern(ushort[] code, byte size, sbyte score)
        {
            Code = code;
            Size = size;
            Score = score;
        }

        public ushort[] Code{ get; }
        public string Pattern => Encoder.Serialize(Code);
        public sbyte Score { get; }
        public byte Size { get; }
    }
}