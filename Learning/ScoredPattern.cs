using Lingua.Core;

namespace Lingua.Learning
{
    public class ScoredPattern
    {
        public ScoredPattern(ushort[] code, sbyte score)
        {
            Code = code;
            Score = score;
        }

        public ushort[] Code{ get; }
        public string Pattern => Encoder.Serialize(Code);
        public sbyte Score { get; }

        public override string ToString()
            => $"{Pattern}:{Score}";
    }
}