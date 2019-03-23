using System.Linq;
using Lingua.Core;
using Xunit;

namespace Lingua.Learning.Test
{
    using Grammar;

    public class TrainableEvaluatorTests
    {
        readonly TrainableEvaluator _evaluator;

        public TrainableEvaluatorTests() => _evaluator = new TrainableEvaluator(new Rearranger());

        [Theory]
        [InlineData("N")]
        [InlineData("NV")]
        public void GivenEmptyTree_UpPatternAddsNodeWithScore_1(string pattern)
        {
            UpdateScore(pattern, 1);
            var leaf = GetLeaf(_evaluator.ScoringTree, pattern.Length);
            Assert.NotNull(leaf);
            Assert.Equal(1, leaf!.Score);
        }

        [Theory]
        [InlineData("N")]
        [InlineData("NV")]
        public void UpAndDownPatternAreSymetrical(string pattern)
        {
            UpdateScore(pattern, 1);
            UpdateScore(pattern, -1);
            Assert.Empty(_evaluator.ScoringTree.Children);
        }

        private static ScoreTreeNode? GetLeaf(ScoreTreeNode node, int depth)
            => node == null
                ? null
                : depth <= 0
                    ? node
                    : GetLeaf(node.Children.FirstOrDefault(), depth - 1);

        private void UpdateScore(string pattern, sbyte score)
            => _evaluator.UpdateScore(Encoder.Encode(pattern).ToArray(), score);
    }
}