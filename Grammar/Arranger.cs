using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    public class Arranger : IEquatable<Arranger>
    {
        public Arranger(string pattern, byte[] order)
        {
            Pattern = pattern;
            Code = Encoder.Encode(Encoder.Deserialize(pattern)).ToArray();
            Order = order;
        }

        public string Pattern { get; }
        private ushort[] Code { get; }
        public byte[] Order { get; }

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