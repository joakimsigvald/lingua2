using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public class Arranger
    {
        public Arranger(Arrangement arrangement)
            => Arrangement = arrangement;

        public Arrangement Arrangement { get; }

        public IEnumerable<ITranslation> Arrange(IList<ITranslation> input)
            => ArrangeSegments(input).SelectMany(x => x.Select(y => y));

        public override string ToString()
            => Arrangement.ToString();

        private IEnumerable<IEnumerable<ITranslation>> ArrangeSegments(ICollection<ITranslation> input)
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

        private IEnumerable<ITranslation> Arrange(ITranslation[] segment)
        {
            var tokens = segment.Select(t => t.From).ToArray();
            var codedSegment = Encoder.Encode(tokens).ToArray();
            if (!Encoder.Matches(codedSegment, Arrangement.Code))
                return null;
            var rearrangedSegment = Rearrange(segment).ToList();
            return rearrangedSegment;
        }

        private IEnumerable<ITranslation> Rearrange(IList<ITranslation> segment)
            => Arrangement.Order.Select(i => segment[i]);
    }
}