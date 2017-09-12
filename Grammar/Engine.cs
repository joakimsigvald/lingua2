using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;

namespace Lingua.Grammar
{
    using Core;

    public class Engine : IGrammar
    {
        private const int Horizon = 6;

        private static readonly Dictionary<string, string> Rearrangements = new Dictionary<string, string>
        {
            { "X1Vd", "2"}
        };

        private static readonly IList<Arranger> Arrangers = Rearrangements.Select(sp => new Arranger(sp.Key, sp.Value)).ToList();

        public IEnumerable<Translation> Reduce(IList<TreeNode<Translation>> possibilities)
            => Arrange(Choose(possibilities));

        private static IEnumerable<Translation> Choose(IList<TreeNode<Translation>> remaining)
        {
            IList<Translation> previous = new List<Translation>();
            while (remaining.Any())
            {
                var child = ReduceNext(previous.Take(Horizon).Reverse().ToList(), remaining);
                previous.Insert(0, child.Value);
                remaining = child.Children.ToList();
            }
            var sequence = previous.Reverse();
            return sequence;
        }

        private static IEnumerable<Translation> Arrange(IEnumerable<Translation> translations)
            => Arrangers
            .Aggregate(translations
                , (input, arranger) => arranger
                .Arrange(input.ToList()));

        private static TreeNode<Translation> ReduceNext(ICollection<Translation> previous,
            IEnumerable<TreeNode<Translation>> next)
        {
            var scoredTranslations = next.SelectMany(node => Expand(node, LookForward(previous)))
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

        private static int LookForward(ICollection<Translation> previous)
            => Math.Max(2, Horizon - previous.Count);

        private static int ComputeScore(IEnumerable<Translation> translations)
            => Scorer.Compute(translations.Select(t => t.From).ToArray());

        private static IEnumerable<IList<TreeNode<Translation>>> Expand(TreeNode<Translation> node, int depth)
            => (depth > 0 && node.Children.Any()
                    ? node.Children
                        .SelectMany(child => Expand(child, depth - child.Value.TokenCount))
                    : new[] {new TreeNode<Translation>[0]})
                .Select(seq => seq.Prepend(node).ToList());
    }
}