using System.Linq;
using Lingua.Grammar;
using Lingua.Testing;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class TrainableEvaluatorTests
    {
        [TestCase("N")]
        [TestCase("NV")]
        public void GivenEmptyTree_UpPatternAddsNodeWithScore_1(string pattern)
        {
            var evaluator = new TrainableEvaluator();
            evaluator.UpPattern(pattern);
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
            evaluator.UpPattern(pattern);
            evaluator.DownPattern(pattern);
            Assert.That(evaluator.ScoringTree.Children, Is.Empty);
        }
    }
}