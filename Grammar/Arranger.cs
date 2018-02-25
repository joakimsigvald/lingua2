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

        public int Length => Arrangement.Length;

        public (ITranslation[] arrangement, int length) Arrange(ITranslation[] segment)
        {
            var tokens = segment.Select(t => t.From).ToArray();
            var codedSegment = Encoder.Encode(tokens).ToArray();
            return !Encoder.Matches(codedSegment, Arrangement.Code) 
                ? (null, 1) 
                : (Rearrange(segment).ToArray(), Arrangement.Length);
        }

        public override string ToString()
            => Arrangement.ToString();

        private IEnumerable<ITranslation> Rearrange(IList<ITranslation> segment)
            => Arrangement.Order.Select(i => segment[i]);
    }
}