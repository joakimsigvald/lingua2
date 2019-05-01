using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    public class Expander
    {
        private readonly IList<ITranslation[]>[] _expansions;
        private readonly IList<ITranslation[]> _possibilities;
        private readonly int _horizon;

        public Expander(IList<ITranslation[]> possibilities, int horizon = int.MaxValue)
        {
            _possibilities = possibilities;
            _horizon = horizon;
            _expansions = new IList<ITranslation[]>[Math.Min(horizon, _possibilities.Count)];
        }

        public IEnumerable<ITranslation[]> Expand() 
            => Expand(0, _horizon).Select(seq => seq.ToArray());

        private IEnumerable<ITranslation[]> Expand(int offset, int todo)
        {
            if (offset == _possibilities.Count)
                return new[] {new ITranslation[0]};
            if (todo > 0)
                return offset >= _expansions.Length
                    ? DoExpand(offset, todo).ToList()
                    : (_expansions[offset]
                       ?? (_expansions[offset] = DoExpand(offset, todo).ToList()));
            return new[] {new ITranslation[0]};
        }

        private IEnumerable<ITranslation[]> DoExpand(int offset, int todo)
        {
            var futures = new List<ITranslation[]>();
            foreach (var first in _possibilities[offset])
            {
                var continuations = Expand(offset + first.WordCount, todo - 1);
                futures.AddRange(continuations.Select(continuation => continuation.Prepend(first).ToArray()));
            }
            return futures;
        }
    }
}