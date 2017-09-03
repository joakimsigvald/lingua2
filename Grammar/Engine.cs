using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public class Engine : IGrammar
    {
        private const int LookBack = 4;
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
            var sequence = previous.Reverse();
            return sequence;
        }

        private static TreeNode<Translation> ReduceNext(ICollection<Translation> previous,
            IEnumerable<TreeNode<Translation>> next)
        {
            var scoredTranslations = next.SelectMany(node => Expand(node, GetHorizon(previous)))
                .Select(seq => new
                {
                    node = seq.First(),
                    score = ComputeScore(previous.Concat(seq.Select(node => node.Value)))
                })
                .OrderByDescending(scoredNode => scoredNode.score).
                ToArray();
            return scoredTranslations
                .Select(scoredNode => scoredNode.node)
                .First();
        }

        private static int GetHorizon(ICollection<Translation> previous)
            => LookForward + LookBack - previous.Count;

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