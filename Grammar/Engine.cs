using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public class Engine : IGrammar
    {
        private const int LookBack = 2;
        private const int LookForward = 2;

        public IEnumerable<Translation> Reduce(IList<TreeNode<Translation>> remaining)
        {
            IList<Translation> previous = new List<Translation>();
            while (remaining.Any())
            {
                var child = ReduceNext(previous.Take(LookBack).Reverse().ToList(), remaining);
                previous.Insert(0, child.Value);
                remaining = child.Children.ToList();
            }
            return previous.Reverse();
        }

        private static TreeNode<Translation> ReduceNext(IList<Translation> previous,
            IEnumerable<TreeNode<Translation>> next)
            => next.SelectMany(node => Expand(node, LookForward))
                .Select(seq => new
                {
                    node = seq.First(),
                    score = ComputeScore(previous.Concat(seq.Select(node => node.Value)))
                })
                .OrderByDescending(scoredNode => scoredNode.score)
                .Select(scoredNode => scoredNode.node)
                .First();

        private static int ComputeScore(IEnumerable<Translation> translations)
            => Scorer.Compute(translations.Select(t => t.From));

        private static IEnumerable<IList<TreeNode<Translation>>> Expand(TreeNode<Translation> node, int depth)
            => (depth > 0 && node.Children.Any()
                    ? node.Children
                        .SelectMany(child => Expand(child, depth - child.Value.TokenCount))
                    : new[] {new TreeNode<Translation>[0]})
                .Select(seq => seq.Prepend(node).ToList());
    }
}