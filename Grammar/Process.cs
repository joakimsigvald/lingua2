using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    internal class Process
    {
        private readonly TranslationTreeNode _possibilities;
        private const int Horizon = 6;
        private readonly IReason _reason = new Reason();
        private Translation[] _selection;
        private static readonly Evaluator Evaluator = new Evaluator();

        internal static (Translation[] Translations, IReason Reason) Execute(TranslationTreeNode possibilities)
        {
            var process = new Process(possibilities);
            process.Reduce();
            return (process._selection, process._reason);
        }

        private Process(TranslationTreeNode possibilities)
            => _possibilities = possibilities;

        private void Reduce()
            => _selection = Choose(_possibilities).Skip(1).ToArray();

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
            var evaluatedTranslations = next.SelectMany(node => Expand(node, Horizon))
                .Select(future => new
                {
                    current = future.First(),
                    evaluation = Evaluator.Evaluate(GetCodeToAnalyze(pastReversed, future))
                })
                .OrderByDescending(scoredNode => scoredNode.evaluation.Score).
                ToArray();
            _reason.Add(evaluatedTranslations.Select(et => et.evaluation));
            return evaluatedTranslations.First().current;
        }

        private static ushort[] GetCodeToAnalyze(IEnumerable<ushort> pastReversed, IEnumerable<TranslationTreeNode> future)
            => pastReversed.Concat(future.Select(node => node.Code)).ToArray();

        private static IEnumerable<IList<TranslationTreeNode>> Expand(TranslationTreeNode node, int depth)
            => (depth > 0 && node.Children.Any()
                    ? node.Children
                        .SelectMany(child => Expand(child, depth - child.Translation.WordCount))
                    : new[] {new TranslationTreeNode[0]})
                .Select(seq => seq.Prepend(node).ToList());
    }
}