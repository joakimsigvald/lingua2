using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;
    using Core.Tokens;

    internal class Process
    {
        private const int Horizon = 6;

        private readonly IList<ITranslation[]> _possibilities;
        private Reason _reason;
        private readonly List<IEvaluation> _evaluations = new List<IEvaluation>();
        private ITranslation[] _translations;
        private readonly IEvaluator _evaluator;
        private IList<ITranslation[]> _futures;
        private bool _futureKnown;
        private int _offset;
        private int _futureOffset;
        private readonly IList<ushort> _previous = new List<ushort> {Start.Code};

        internal static (ITranslation[] Translations, IReason Reason) Execute(IEvaluator evaluator,
            IList<ITranslation[]> possibilities)
        {
            var process = new Process(evaluator, possibilities);
            process.Reduce();
            return (process._translations, process._reason);
        }

        private Process(IEvaluator evaluator, IList<ITranslation[]> possibilities)
        {
            _evaluator = evaluator;
            _possibilities = possibilities;
        }

        private void Reduce()
        {
            _translations = Choose().ToArray();
            _reason = new Reason(Encoder.Encode(_translations.Select(t => t.From)).ToArray(), _evaluations);
        }

        private IEnumerable<ITranslation> Choose()
        {
            ITranslation translation = null;
            while (_offset < _possibilities.Count)
            {
                yield return translation = ChooseNext(translation);
                _previous.Insert(0, translation.Code);
                _offset += translation.WordCount;
                if (_futureKnown)
                    _futureOffset++;
            }
        }

        private ITranslation ChooseNext(ITranslation translation)
            => _futureKnown
                ? _futures.First()[_futureOffset]
                : SelectNext(translation);

        private ITranslation SelectNext(ITranslation translation)
            => _possibilities[_offset].Length == 1
                ? _possibilities[_offset].Single()
                : FindNext(translation);

        private ITranslation FindNext(ITranslation translation)
        {
            _futures = GetNextFutures(translation).ToList();
            var pastReversed = _previous.Reverse().ToArray();
            var evaluatedFutures = EvaluateFutures(pastReversed);
            _evaluations.AddRange(evaluatedFutures.Select(et => et.evaluation));
            _futures = evaluatedFutures.Select(ef => ef.future).ToList();
            return _futures.First()[_futureOffset];
        }

        private (ITranslation[] future, Evaluation evaluation)[] EvaluateFutures(ushort[] pastReversed)
            => _futures
                .Select(future => EvaluateFuture(pastReversed, future))
                .OrderByDescending(scoredTranslation => scoredTranslation.evaluation.Score).ToArray();

        private (ITranslation[] future, Evaluation evaluation) EvaluateFuture(
            ICollection<ushort> pastReversed, ITranslation[] future)
        {
            var commonLength = pastReversed.Count;
            var pattern = GetCodeWithinHorizon(pastReversed
                , _futureOffset == 0 ? future : future.Skip(_futureOffset));
            var evaluation = _evaluator.Evaluate(pattern, commonLength);
            return (future, evaluation);
        }

        private IEnumerable<ITranslation[]> GetNextFutures(ITranslation translation)
            => _futureKnown
                ? _futures.Where(future => future[_futureOffset - 1] == translation)
                : new Expander(_possibilities.Skip(_offset).ToList(), Horizon).Expand(out _futureKnown);

        private static ushort[] GetCodeWithinHorizon(IEnumerable<ushort> pastReversed,
            IEnumerable<ITranslation> future)
            => pastReversed.Concat(future.Select(node => node.Code)).ToArray();

    }
}