using Lingua.Core;
using System;

namespace Lingua.Learning
{
    public class ScoredPattern : IEquatable<ScoredPattern?>
    {
        public ScoredPattern(Code code, sbyte score)
        {
            Code = code;
            Score = score;
        }

        public Code Code { get; }
        public sbyte Score { get; }
        public string Pattern => Code.Pattern;
        public ushort[] ReversedCode => Code.ReversedCode;

        public override string ToString()
            => $"{Pattern}:{Score}";

        public bool Equals(ScoredPattern? other)
            => other != null
               && Code == other.Code
               && Score == other.Score;

        public override bool Equals(object obj)
            => Equals(obj as ScoredPattern);

        public override int GetHashCode() => Code.GetHashCode();
    }
}