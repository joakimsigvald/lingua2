using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    public class Expander
    {
        private readonly IList<ITranslation[]>[] _expansions;
        private readonly IDecomposition _filteredDecomposition;

        public Expander(IDecomposition filteredDecomposition)
        {
            _filteredDecomposition = filteredDecomposition;
            _expansions = new IList<ITranslation[]>[_filteredDecomposition.Length];
        }

        public IEnumerable<ITranslation[]> Expand()
            => Expand(0).Select(seq => seq.ToArray());

        private IEnumerable<ITranslation[]> Expand(int offset)
            => offset == _filteredDecomposition.Length
             ? new[] { new ITranslation[0] }
            : offset >= _expansions.Length
                ? DoExpand(offset).ToList()
                : (_expansions[offset]
                   ?? (_expansions[offset] = DoExpand(offset).ToList()));

        private IEnumerable<ITranslation[]> DoExpand(int offset)
        {
            var futures = new List<ITranslation[]>();
            foreach (var first in _filteredDecomposition[offset])
            {
                var continuations = Expand(offset + first.Last().WordCount);
                futures.AddRange(continuations.Select(continuation => first.Concat(continuation).ToArray()));
            }
            return futures;
        }
    }
}