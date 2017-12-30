using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Extensions;

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

        public IEnumerable<ITranslation[]> Expand(out bool futureKnown)
        {
            futureKnown = true;
            return Expand(0, _horizon, ref futureKnown)
                .Select(seq => seq.ToArray());
        }

        private IEnumerable<ITranslation[]> Expand(int offset, int todo, ref bool futureKnown)
        {
            if (offset == _possibilities.Count)
                return new[] { new ITranslation[0] };
            if (todo > 0)
                return _expansions[offset]
                       ?? (_expansions[offset] = DoExpand(offset, todo, ref futureKnown).ToList());
            futureKnown = false;
            return new[] { new ITranslation[0] };
        }

        private IEnumerable<ITranslation[]> DoExpand(int offset, int todo, ref bool cutoff)
        {
            var futures = new List<ITranslation[]>();
            foreach (var first in _possibilities[offset])
            {
                var continuations = Expand(offset + first.WordCount, todo - 1, ref cutoff);
                futures.AddRange(continuations.Select(continuation => continuation.Prepend(first).ToArray()));
            }
            return futures;
        }
    }
}