using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    internal class Arranger
    {
        public Arranger(string from, string to)
        {
            From = Encoder.Encode(Encoder.Deserialize(from)).ToArray();
            To = to.Select(c => (byte)(c - 49)).ToArray();
        }

        private ushort[] From { get; }
        private byte[] To { get; }

        public IEnumerable<Translation> Arrange(IList<Translation> input)
            => ArrangeSegments(input).SelectMany(x => x.Select(y => y));

        private IEnumerable<IEnumerable<Translation>> ArrangeSegments(ICollection<Translation> input)
        {
            for (var i = 0; i < input.Count; i++)
            {
                var segment = input
                    .Skip(i).Take(From.Length)
                    .ToArray();
                var arrangedSegment = Arrange(segment);
                if (arrangedSegment != null)
                {
                    i += From.Length - 1;
                    yield return arrangedSegment;
                }
                else yield return segment.Take(1);
            }
        }

        private IEnumerable<Translation> Arrange(Translation[] segment)
        {
            var tokens = segment.Select(t => t.From).ToArray();
            var codedSegment = Encoder.Encode(tokens).ToArray();
            if (!Encoder.Matches(codedSegment, From))
                return null;
            var rearrangedSegment = Rearrange(segment).ToList();
            return rearrangedSegment;
        }

        private IEnumerable<Translation> Rearrange(IList<Translation> segment)
            => To.Select(i => segment[i]);
    }
}