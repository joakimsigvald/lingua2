using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public class Arranger
    {
        public Arranger(Arrangement arrangement)
            => Arrangement = arrangement;

        private Arrangement Arrangement { get; }

        public IEnumerable<Translation> Arrange(IList<Translation> input)
            => ArrangeSegments(input).SelectMany(x => x.Select(y => y));

        public override string ToString()
            => Arrangement.ToString();

        private IEnumerable<IEnumerable<Translation>> ArrangeSegments(ICollection<Translation> input)
        {
            for (var i = 0; i < input.Count; i++)
            {
                var segment = input
                    .Skip(i).Take(Arrangement.Code.Length)
                    .ToArray();
                var arrangedSegment = Arrange(segment);
                if (arrangedSegment != null)
                {
                    i += Arrangement.Code.Length - 1;
                    yield return arrangedSegment;
                }
                else yield return segment.Take(1);
            }
        }

        private IEnumerable<Translation> Arrange(Translation[] segment)
        {
            var tokens = segment.Select(t => t.From).ToArray();
            var codedSegment = Encoder.Encode(tokens).ToArray();
            if (!Encoder.Matches(codedSegment, Arrangement.Code))
                return null;
            var rearrangedSegment = Rearrange(segment).ToList();
            return rearrangedSegment;
        }

        private IEnumerable<Translation> Rearrange(IList<Translation> segment)
            => Arrangement.Order.Select(i => segment[i]);
    }
}