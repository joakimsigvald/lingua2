using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;
    using Core.Extensions;
    using Core.Tokens;

    internal class Process
    {
        private readonly IList<Translation[]> _possibilities;
        private const int Horizon = 6;
        private Reason _reason;
        private readonly List<IEvaluation> _evaluations = new List<IEvaluation>();
        private Translation[] _translations;
        private readonly IEvaluator _evaluator;
        private IList<Translation[]> _futures;
        private bool _futureKnown;
        private int _offset;
        private int _futureOffset;
        private readonly IList<ushort> _previous = new List<ushort> {Start.Code};

        internal static (Translation[] Translations, IReason Reason) Execute(IEvaluator evaluator,
            IList<Translation[]> possibilities)
        {
            var process = new Process(evaluator, possibilities);
            process.Reduce();
            return (process._translations, process._reason);
        }

        private Process(IEvaluator evaluator, IList<Translation[]> possibilities)
        {
            _evaluator = evaluator;
            _possibilities = possibilities;
        }

        private void Reduce()
        {
            _translations = Choose().ToArray();
            _reason = new Reason(Encoder.Encode(_translations.Select(t => t.From)).ToArray(), _evaluations);
        }

        private IEnumerable<Translation> Choose()
        {
            Translation translation = null;
            while (_offset < _possibilities.Count)
            {
                yield return translation = ChooseNext(translation);
                _previous.Insert(0, translation.Code);
                _offset += translation.WordCount;
                if (_futureKnown)
                    _futureOffset++;
            }
        }

        private Translation ChooseNext(Translation translation)
            => _futureKnown
                ? _futures.First()[_futureOffset]
                : SelectNext(translation);

        private Translation SelectNext(Translation translation)
            => _possibilities[_offset].Length == 1
                ? _possibilities[_offset].Single()
                : FindNext(translation);

        private Translation FindNext(Translation translation)
        {
            _futures = GetNextFutures(translation).ToList();
            var pastReversed = _previous.Take(Horizon).Reverse().ToArray();
            var evaluatedFutures = EvaluateFutures(pastReversed);
            _evaluations.AddRange(evaluatedFutures.Select(et => et.evaluation));
            _futures = evaluatedFutures.Select(ef => ef.future).ToList();
            return _futures.First()[_futureOffset];
        }

        private (Translation[] future, Evaluation evaluation)[] EvaluateFutures(ushort[] pastReversed)
            => _futures
                .Select(future => EvaluateFuture(pastReversed, future))
                .OrderByDescending(scoredTranslation => scoredTranslation.evaluation.Score).ToArray();

        private (Translation[] future, Evaluation evaluation) EvaluateFuture(IEnumerable<ushort> pastReversed,
            Translation[] future)
            => (future: future
                , evaluation: _evaluator.Evaluate(GetCodeWithinHorizon(pastReversed
                    , _futureOffset == 0 ? future : future.Skip(_futureOffset))));

        private IEnumerable<Translation[]> GetNextFutures(Translation translation)
            => _futureKnown
                ? _futures.Where(future => future[_futureOffset - 1] == translation)
                : new Expander(_possibilities.Skip(_offset).ToList()).Expand(out _futureKnown);

        private static ushort[] GetCodeWithinHorizon(IEnumerable<ushort> pastReversed,
            IEnumerable<Translation> future)
            => pastReversed.Concat(future.Select(node => node.Code)).ToArray();

        private class Expander
        {
            private readonly IList<Translation[]>[] _expansions;
            private readonly IList<Translation[]> _possibilities;

            public Expander(IList<Translation[]> possibilities)
            {
                _possibilities = possibilities;
                _expansions = new IList<Translation[]>[Math.Min(Horizon, _possibilities.Count)];
            }

            public IEnumerable<Translation[]> Expand(out bool futureKnown)
            {
                futureKnown = true;
                return Expand(0, Horizon, ref futureKnown)
                    .Select(seq => seq.ToArray());
            }

            private IEnumerable<Translation[]> Expand(int offset, int todo, ref bool futureKnown)
            {
                if (offset == _possibilities.Count)
                    return new[] { new Translation[0] };
                if (todo > 0)
                    return _expansions[offset] 
                        ?? (_expansions[offset] = DoExpand(offset, todo, ref futureKnown).ToList());
                futureKnown = false;
                return new[] { new Translation[0] };
            }

            private IEnumerable<Translation[]> DoExpand(int offset, int todo, ref bool cutoff)
            {
                var futures = new List<Translation[]>();
                foreach (var first in _possibilities[offset])
                {
                    var continuations = Expand(offset + first.WordCount, todo - 1, ref cutoff);
                    futures.AddRange(continuations.Select(continuation => continuation.Prepend(first).ToArray()));
                }
                return futures;
            }
        }
    }
}