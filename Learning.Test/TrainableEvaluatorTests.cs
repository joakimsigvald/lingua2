using System.Linq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    using Grammar;
    using Testing;

    [TestFixture]
    public class TrainableEvaluatorTests
    {
        [TestCase("N")]
        [TestCase("NV")]
        public void GivenEmptyTree_UpPatternAddsNodeWithScore_1(string pattern)
        {
            var evaluator = new TrainableEvaluator();
            evaluator.UpdateScore(pattern, 1);
            var leaf = GetLeaf(evaluator.ScoringTree, pattern.Length);
            Assert.That(leaf?.Score, Is.EqualTo(1));
        }

        private static ScoreTreeNode GetLeaf(ScoreTreeNode node, int depth)
            => node == null
                ? null
                : depth <= 0
                    ? node
                    : GetLeaf(node.Children.FirstOrDefault(), depth - 1);

        [TestCase("N")]
        [TestCase("NV")]
        public void UpAndDownPatternAreSymetrical(string pattern)
        {
            var evaluator = new TrainableEvaluator();
            evaluator.UpdateScore(pattern, 1);
            evaluator.UpdateScore(pattern, -1);
            Assert.That(evaluator.ScoringTree.Children, Is.Empty);
        }
    }
}