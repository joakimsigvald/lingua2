using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    internal class Process
    {
        private readonly TreeNode<Tuple<Translation, ushort>> _possibilities;
        private const int Horizon = 6;
        private readonly IReason _reason = new Reason();
        private IEnumerable<Translation> _selection;
        private static readonly Evaluator Evaluator = new Evaluator();

        internal static (IEnumerable<Translation> Translations, IReason Reason) Execute(TreeNode<Tuple<Translation, ushort>> possibilities)
        {
            var process = new Process(possibilities);
            process.Reduce();
            return (process._selection, process._reason);
        }

        private Process(TreeNode<Tuple<Translation, ushort>> possibilities)
            => _possibilities = possibilities;

        private void Reduce()
            => _selection = Choose(_possibilities).Skip(1);

        private IEnumerable<Translation> Choose(TreeNode<Tuple<Translation, ushort>> possibilities)
        {
            var remaining = new List<TreeNode<Tuple<Translation, ushort>>> {possibilities};
            IList<ushort> previous = new List<ushort>();
            while (remaining.Any())
            {
                var child = GetNext(previous, remaining);
                yield return child.Value.Item1;
                previous.Insert(0, child.Value.Item2);
                remaining = child.Children.ToList();
            }
        }

        private TreeNode<Tuple<Translation, ushort>> GetNext(IEnumerable<ushort> previous,
            ICollection<TreeNode<Tuple<Translation, ushort>>> next)
            => next.Count > 1 ? FindNext(previous, next) : next.Single();

        private TreeNode<Tuple<Translation, ushort>> FindNext(IEnumerable<ushort> previous,
            IEnumerable<TreeNode<Tuple<Translation, ushort>>> next)
        {
            var previousReversed = previous.Take(Horizon).Reverse().ToList();
            var evaluatedTranslations = next.SelectMany(node => Expand(node, Horizon))
                .Select(seq => new
                {
                    node = seq.First(),
                    evaluation = Evaluate(previousReversed.Concat(seq.Select(node => node.Value.Item2)))
                })
                .OrderByDescending(scoredNode => scoredNode.evaluation.Score).
                ToArray();
            _reason.Add(evaluatedTranslations.Select(et => et.evaluation));
            return evaluatedTranslations
                .Select(scoredNode => scoredNode.node)
                .First();
        }

        private static Evaluation Evaluate(IEnumerable<ushort> sequence)
            => Evaluator.Evaluate(sequence.ToArray());

        private static IEnumerable<IList<TreeNode<Tuple<Translation, ushort>>>> Expand(TreeNode<Tuple<Translation, ushort>> node, int depth)
            => (depth > 0 && node.Children.Any()
                    ? node.Children
                        .SelectMany(child => Expand(child, depth - child.Value.Item1.WordCount))
                    : new[] {new TreeNode<Tuple<Translation, ushort>>[0]})
                .Select(seq => seq.Prepend(node).ToList());
    }
}