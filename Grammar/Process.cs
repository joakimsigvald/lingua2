using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    internal class Process
    {
        private readonly TranslationTreeNode _possibilities;
        private const int Horizon = 6;
        private Reason _reason;
        private readonly List<IEvaluation> _evaluations = new List<IEvaluation>();
        private Translation[] _translations;
        private readonly IEvaluator _evaluator;

        internal static (Translation[] Translations, IReason Reason) Execute(IEvaluator evaluator,
            TranslationTreeNode possibilities)
        {
            var process = new Process(evaluator, possibilities);
            process.Reduce();
            return (process._translations, process._reason);
        }

        private Process(IEvaluator evaluator, TranslationTreeNode possibilities)
        {
            _evaluator = evaluator;
            _possibilities = possibilities;
        }

        private void Reduce()
        {
            _translations = Choose(_possibilities).Skip(1).ToArray();
            _reason = new Reason(Encoder.Encode(_translations.Select(t => t.From)).ToArray(), _evaluations);
        }

        private IEnumerable<Translation> Choose(TranslationTreeNode possibilities)
        {
            var remaining = new List<TranslationTreeNode> {possibilities};
            IList<ushort> previous = new List<ushort>();
            while (remaining.Any())
            {
                var child = GetNext(previous, remaining);
                yield return child.Translation;
                previous.Insert(0, child.Code);
                remaining = child.Children.ToList();
            }
        }

        private TranslationTreeNode GetNext(IEnumerable<ushort> previous,
            ICollection<TranslationTreeNode> next)
            => next.Count > 1 ? FindNext(previous, next) : next.Single();

        private TranslationTreeNode FindNext(IEnumerable<ushort> previous,
            IEnumerable<TranslationTreeNode> next)
        {
            var pastReversed = previous.Take(Horizon).Reverse().ToList();
            var evaluatedTranslations = next.SelectMany(node => node.Expand(Horizon))
                .Select(future => new
                {
                    current = future.First(),
                    evaluation = _evaluator.Evaluate(GetCodeWithinHorizon(pastReversed, future))
                })
                .OrderByDescending(scoredNode => scoredNode.evaluation.Score).ToArray();
            _evaluations.AddRange(evaluatedTranslations.Select(et => et.evaluation));
            return evaluatedTranslations.First().current;
        }

        private static ushort[] GetCodeWithinHorizon(IEnumerable<ushort> pastReversed,
            IEnumerable<TranslationTreeNode> future)
            => pastReversed.Concat(future.Select(node => node.Code)).ToArray();
    }
}