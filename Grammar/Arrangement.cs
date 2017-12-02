using System;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public class Arrangement : IEquatable<Arrangement>
    {
        public Arrangement(ushort[] code, byte[] order)
            : this(Encoder.Serialize(code), code, order)
        {
        }

        private Arrangement(string pattern, byte[] order)
            : this(pattern, Encoder.Encode(Encoder.Deserialize(pattern)).ToArray(), order)
        {
        }

        private Arrangement(string pattern, ushort[] code, byte[] order)
        {
            Pattern = pattern;
            Code = code;
            Order = order;
        }

        private string Pattern { get; }
        public ushort[] Code { get; }
        public byte[] Order { get; }
        public int Length => Code.Length;
        public bool IsInOrder => Order.Select((n, i) => n - i).All(dif => dif == 0);

        public string Serialize()
            => $"{Pattern}:{string.Join("", Order.Select(n => n + 1))}";

        public static Arrangement Deserialize(string serial)
        {
            var parts = serial.Split(':');
            return new Arrangement(parts[0], parts[1].Select(c => (byte)(c - 49)).ToArray());
        }

        public bool Equals(Arrangement other)
            => other?.Pattern == Pattern;

        public override bool Equals(object obj)
            => Equals(obj as Arrangement);

        public override int GetHashCode()
            => Pattern.GetHashCode();

        public override string ToString()
            => Serialize();
    }
}