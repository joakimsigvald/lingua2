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
            var offset = 0;
            IList<ushort> previous = new List<ushort> {Start.Code};
            while (offset < _possibilities.Count)
            {
                var translation = GetNext(previous, offset);
                yield return translation;
                previous.Insert(0, translation.Code);
                offset += translation.WordCount;
            }
        }

        private Translation GetNext(IEnumerable<ushort> previous, int offset)
            => _possibilities[offset].Length > 1 
            ? FindNext(previous, offset) 
            : _possibilities[offset].Single();

        private Translation FindNext(IEnumerable<ushort> previous, int offset)
        {
            var pastReversed = previous.Take(Horizon).Reverse().ToList();
            var futures = Expand(offset);
            var evaluatedTranslations = futures
                .Select(future => new
                {
                    current = future.First(),
                    evaluation = _evaluator.Evaluate(GetCodeWithinHorizon(pastReversed, future))
                })
                .OrderByDescending(scoredNode => scoredNode.evaluation.Score).ToArray();
            _evaluations.AddRange(evaluatedTranslations.Select(et => et.evaluation));
            return evaluatedTranslations.First().current;
        }

        private IEnumerable<IList<Translation>> Expand(int offset)
            => Expand(offset, Horizon)
                .Select(seq => seq.ToArray());

        private IEnumerable<IEnumerable<Translation>> Expand(int offset, int todo)
            => offset < _possibilities.Count && todo > 0
                ? _possibilities[offset].SelectMany(first => Expand(offset + first.WordCount, todo - 1)
                    .Select(rest => rest.Prepend(first)))
                : new[] { new Translation[0] };

        private static ushort[] GetCodeWithinHorizon(IEnumerable<ushort> pastReversed,
            IEnumerable<Translation> future)
            => pastReversed.Concat(future.Select(node => node.Code)).ToArray();
    }
}