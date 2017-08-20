using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    /// <Types>
    /// A: Adjective
    /// V: Verb
    /// N: Noun
    /// T: ArTicle
    /// Q: quantifier (number)
    /// .: Terminator
    /// ,: Separator
    /// </Types>

    /// <Modifiers>
    /// d: definite
    /// n: plural
    /// p: possessive
    /// </Modifiers>

    public class Engine : IGrammar
    {
        private const int LookBack = 2;
        private const int LookForward = 2;

        private static readonly IDictionary<string, int> ScoredPatterns = new Dictionary<string, int>
        {
            { "TdNd", 1},
            { "TdNdn", 1},
            { "TdNdp", 1},
            { "TdNdnp", 1},
            { "QnNn", 1},
        };

        private static readonly IList<Scorer> Scorers = ScoredPatterns.Select(sp => new Scorer
        {
            Pattern = Encoder.Code(Encoder.Deserialize(sp.Key)).ToArray(),
            Score = sp.Value
        }).ToList();

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
                    score = Score(previous.Concat(seq.Select(node => node.Value)))
                })
                .OrderByDescending(scoredNode => scoredNode.score)
                .Select(scoredNode => scoredNode.node)
                .First();

        private static IEnumerable<IList<TreeNode<Translation>>> Expand(TreeNode<Translation> node, int depth)
            => (depth > 0 && node.Children.Any()
                    ? node.Children
                        .SelectMany(child => Expand(child, depth - child.Value.TokenCount))
                    : new[] {new TreeNode<Translation>[0]})
                .Select(seq => seq.Prepend(node).ToList());

        private static int Score(IEnumerable<Translation> translations)
        {
            var tokens = translations.Select(t => t.From).ToArray();
            var str = Encoder.Serialize(tokens);
            var code = Encoder.Code(tokens).ToArray();
            var score = Scorers.Sum(s => s.CountMatches(code) * s.Score);
            return score;
        }
    }
}