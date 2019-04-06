using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    public class Code : IEquatable<Code?>, IComparable<Code>
    {
        public readonly ushort[] ReversedCode;
        private readonly int _hashCode;

        public Code(IEnumerable<ushort> reversedCode)
        {
            ReversedCode = reversedCode.ToArray();
            _hashCode = ReversedCode.Aggregate(0, (a, b) => a + b);
        }

        public int Length => ReversedCode.Length;

        public ushort[] SkipFirst() => ReversedCode[..^1];

        private ushort this[int i]
        {
            get => ReversedCode[i];
        }

        public Code Take(int i) => new Code(ReversedCode.Take(i));
        public string Pattern => Encoder.Serialize(ReversedCode.Reverse());

        public static bool operator >(Code left, Code right)
        {
            if (left.Length > right.Length)
                return true;
            if (left.Length < right.Length)
                return false;
            for (int i = 0; i < left.Length; i++)
            {
                var lc = left[i];
                var rc = right[i];
                if (lc > rc)
                    return true;
                if (lc < rc)
                    return false;
            }
            return false;
        }

        public static bool operator <(Code left, Code right) => right > left;

        public static bool operator >=(Code left, Code right) => left == right || left > right;

        public static bool operator <=(Code left, Code right) => right >= left;

        public static bool operator ==(Code left, Code right) => left.Equals(right);

        public static bool operator !=(Code left, Code right) => !left.Equals(right);

        public ushort Last() => ReversedCode.First();

        public ushort First() => ReversedCode.Last();

        public bool Equals(Code? other)
            => !(other is null) && ReversedCode.SequenceEqual(other.ReversedCode);

        public override bool Equals(object obj)
            => Equals(obj as Code);

        public override string ToString()
            => Encoder.Serialize(ReversedCode.Reverse());

        public override int GetHashCode()
            => _hashCode;

        public int CompareTo(Code other)
            => this == other ? 0 : this > other ? 1 : -1;
    }
}