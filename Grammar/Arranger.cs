using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public class Arranger : IEquatable<Arranger>
    {
        public Arranger(string pattern, byte[] order)
            : this(pattern, Encoder.Encode(Encoder.Deserialize(pattern)).ToArray(), order)
        {
        }

        public Arranger(ushort[] code, byte[] order)
            : this(Encoder.Serialize(code), code, order)
        {
        }

        private Arranger(string pattern, ushort[] code, byte[] order)
        {
            Pattern = pattern;
            Code = code;
            Order = order;
        }

        private string Pattern { get; }
        private ushort[] Code { get; }
        private byte[] Order { get; }
        public int Length => Code.Length;

        public IEnumerable<Translation> Arrange(IList<Translation> input)
            => ArrangeSegments(input).SelectMany(x => x.Select(y => y));

        private IEnumerable<IEnumerable<Translation>> ArrangeSegments(ICollection<Translation> input)
        {
            for (var i = 0; i < input.Count; i++)
            {
                var segment = input
                    .Skip(i).Take(Code.Length)
                    .ToArray();
                var arrangedSegment = Arrange(segment);
                if (arrangedSegment != null)
                {
                    i += Code.Length - 1;
                    yield return arrangedSegment;
                }
                else yield return segment.Take(1);
            }
        }

        private IEnumerable<Translation> Arrange(Translation[] segment)
        {
            var tokens = segment.Select(t => t.From).ToArray();
            var codedSegment = Encoder.Encode(tokens).ToArray();
            if (!Encoder.Matches(codedSegment, Code))
                return null;
            var rearrangedSegment = Rearrange(segment).ToList();
            return rearrangedSegment;
        }

        private IEnumerable<Translation> Rearrange(IList<Translation> segment)
            => Order.Select(i => segment[i]);

        public bool Equals(Arranger other)
            => other != null && other.Pattern == Pattern;

        public override bool Equals(object obj)
            => Equals(obj as Arranger);

        public override int GetHashCode()
            => Pattern.GetHashCode();
    }
}