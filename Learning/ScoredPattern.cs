using System;
using System.Linq;
using Lingua.Core;

namespace Lingua.Learning
{
    public class ScoredPattern : IEquatable<ScoredPattern>
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

        public bool Equals(ScoredPattern other)
            => other != null
               && Code.SequenceEqual(other.Code)
               && Score == other.Score;

        public override bool Equals(object obj)
            => Equals(obj as ScoredPattern);

        public override int GetHashCode()
            => Code.Aggregate(0, (a, b) => a + b);
    }
}