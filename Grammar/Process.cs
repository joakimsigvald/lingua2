using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    internal class Process
    {
        private readonly LazyTreeNode<Tuple<Translation, ushort>> _possibilities;
        private const int Horizon = 6;
        private readonly IReason _reason = new Reason();
        private IEnumerable<Translation> _selection;
        private static readonly Evaluator Evaluator = new Evaluator();

        internal static (IEnumerable<Translation> Translations, IReason Reason) Execute(LazyTreeNode<Tuple<Translation, ushort>> possibilities)
        {
            var process = new Process(possibilities);
            process.Reduce();
            return (process._selection, process._reason);
        }

        private Process(LazyTreeNode<Tuple<Translation, ushort>> possibilities)
            => _possibilities = possibilities;

        private void Reduce()
            => _selection = Choose(_possibilities).Skip(1);

        private IEnumerable<Translation> Choose(LazyTreeNode<Tuple<Translation, ushort>> possibilities)
        {
            var remaining = new List<LazyTreeNode<Tuple<Translation, ushort>>> {possibilities};
            IList<ushort> previous = new List<ushort>();
            while (remaining.Any())
            {
                var child = GetNext(previous, remaining);
                yield return child.Value.Item1;
                previous.Insert(0, child.Value.Item2);
                remaining = child.Children.ToList();
            }
        }

        private LazyTreeNode<Tuple<Translation, ushort>> GetNext(IEnumerable<ushort> previous,
            ICollection<LazyTreeNode<Tuple<Translation, ushort>>> next)
            => next.Count > 1 ? FindNext(previous, next) : next.Single();

        private LazyTreeNode<Tuple<Translation, ushort>> FindNext(IEnumerable<ushort> previous,
            IEnumerable<LazyTreeNode<Tuple<Translation, ushort>>> next)
        {
            var previousReversed = previous.Take(Horizon).Reverse().ToList();
            var evaluatedTranslations = next.SelectMany(node => Expand(node, Horizon))
                .Select(seq => new
                {
                    node = seq.First(),
                    evaluation = Evaluator.Evaluate(previousReversed.Concat(seq.Select(node => node.Value.Item2)).ToArray())
                })
                .OrderByDescending(scoredNode => scoredNode.evaluation.Score).
                ToArray();
            _reason.Add(evaluatedTranslations.Select(et => et.evaluation));
            return evaluatedTranslations.First().node;
        }

        private static IEnumerable<IList<LazyTreeNode<Tuple<Translation, ushort>>>> Expand(LazyTreeNode<Tuple<Translation, ushort>> node, int depth)
            => (depth > 0 && node.Children.Any()
                    ? node.Children
                        .SelectMany(child => Expand(child, depth - child.Value.Item1.WordCount))
                    : new[] {new LazyTreeNode<Tuple<Translation, ushort>>[0]})
                .Select(seq => seq.Prepend(node).ToList());
    }
}