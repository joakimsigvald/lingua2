using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    internal class Process
    {
        private readonly IList<TreeNode<Translation>> _possibilities;
        private const int Horizon = 6;
        private readonly IReason _reason = new Reason();
        private IEnumerable<Translation> _selection;

        private static readonly Dictionary<string, string> Rearrangements = Loader.LoadRearrangements();

        private static readonly IList<Arranger> Arrangers = Rearrangements.Select(sp => new Arranger(sp.Key, sp.Value)).ToList();

        internal static (IReason Reason, IEnumerable<Translation> Translations) Execute(IList<TreeNode<Translation>> possibilities)
        {
            var process = new Process(possibilities);
            process.Reduce();
            return (process._reason, process._selection);
        }

        private Process(IList<TreeNode<Translation>> possibilities)
            => _possibilities = possibilities;

        private void Reduce()
            => _selection = Arrange(Choose(_possibilities));

        private IEnumerable<Translation> Choose(IList<TreeNode<Translation>> remaining)
        {
            IList<Translation> previous = new List<Translation>();
            while (remaining.Any())
            {
                var child = GetNext(previous, remaining);
                previous.Insert(0, child.Value);
                remaining = child.Children.ToList();
            }
            return previous.Reverse();
        }

        private static IEnumerable<Translation> Arrange(IEnumerable<Translation> translations)
            => Arrangers
            .Aggregate(translations
                , (input, arranger) => arranger
                .Arrange(input.ToList()));

        private TreeNode<Translation> GetNext(IEnumerable<Translation> previous,
            ICollection<TreeNode<Translation>> next)
            => next.Count > 1 ? FindNext(previous, next) : next.Single();

        private TreeNode<Translation> FindNext(IEnumerable<Translation> previous,
            IEnumerable<TreeNode<Translation>> next)
        {
            var previousReversed = previous.Take(Horizon).Reverse().ToList();
            var evaluatedTranslations = next.SelectMany(node => Expand(node, Horizon))
                .Select(seq => new
                {
                    node = seq.First(),
                    evaluation = Evaluate(previousReversed.Concat(seq.Select(node => node.Value)))
                })
                .OrderByDescending(scoredNode => scoredNode.evaluation.Score).
                ToArray();
            _reason.Add(evaluatedTranslations.Select(et => et.evaluation));
            return evaluatedTranslations
                .Select(scoredNode => scoredNode.node)
                .First();
        }

        private static Evaluation Evaluate(IEnumerable<Translation> translations)
            => Scorer.Evaluate(translations.Select(t => t.From).ToArray());

        private static IEnumerable<IList<TreeNode<Translation>>> Expand(TreeNode<Translation> node, int depth)
            => (depth > 0 && node.Children.Any()
                    ? node.Children
                        .SelectMany(child => Expand(child, depth - child.Value.WordCount))
                    : new[] {new TreeNode<Translation>[0]})
                .Select(seq => seq.Prepend(node).ToList());
    }
}