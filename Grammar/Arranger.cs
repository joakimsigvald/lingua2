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

        public (IGrammaton[]? arrangement, int length) Arrange(IGrammaton[] segment)
        {
            var codedSegment = segment.Select(t => t.Code).ToArray();
            return !Encoder.Matches(codedSegment, Arrangement.Code) 
                ? (null, 1) 
                : (Rearrange(segment).ToArray(), Arrangement.Length);
        }

        public override string ToString()
            => Arrangement.ToString();

        private IEnumerable<IGrammaton> Rearrange(IList<IGrammaton> segment)
            => Arrangement.Order.Select(i => segment[i]);
    }
}