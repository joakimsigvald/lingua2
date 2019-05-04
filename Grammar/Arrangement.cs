using System;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;
    using System.Collections.Generic;

    public class Arrangement : IEquatable<Arrangement>
    {
        public Arrangement(ushort[] code, byte[] order)
            : this(Encoder.Serialize(code), code, order)
        {
        }

        public (IGrammaton[]? arrangement, int length) Arrange(IGrammaton[] segment)
        {
            var codedSegment = segment.Select(t => t.Code).ToArray();
            return !Encoder.Matches(codedSegment, Code)
                ? (null, 1)
                : (Rearrange(segment).ToArray(), Length);
        }

        private IEnumerable<IGrammaton> Rearrange(IList<IGrammaton> segment)
            => Order.Select(i => segment[i]);


        private Arrangement(string pattern, byte[] order)
            : this(pattern, Encoder.Encode(Encoder.Deserialize(pattern)).ToArray(), order)
        {
        }

        private Arrangement(string pattern, ushort[] code, byte[] order)
        {
            if (order.Length > order.Distinct().Count())
                throw new InvalidProgramException("Invalid order, cannot repeate word");
            Pattern = pattern;
            Code = code;
            Order = order;
        }

        public string Pattern { get; }
        public ushort[] Code { get; }
        public byte[] Order { get; }
        public int Length => Code.Length;
        public bool IsInPerfectOrder => Order.Select((n, i) => n - i).All(dif => dif == 0);

        public string Serialize()
            => $"{Pattern}:{string.Join("", Order.Select(n => n + 1))}";

        public static Arrangement Deserialize(string serial)
        {
            var parts = serial.Split(':');
            return new Arrangement(parts[0], parts[1].Select(c => (byte)(c - 49)).ToArray());
        }

        public bool Equals(Arrangement other)
            => other.Pattern == Pattern;

        public override bool Equals(object obj)
            => obj is Arrangement arr && Equals(arr);

        public override int GetHashCode()
            => Pattern.GetHashCode();

        public override string ToString()
            => Serialize();
    }
}