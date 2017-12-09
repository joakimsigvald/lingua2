using System.Linq;
using Lingua.Core;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    using Grammar;

    [TestFixture]
    public class TrainableEvaluatorTests
    {
        TrainableEvaluator _evaluator;

        [SetUp]
        public void SetUp()
        {
            _evaluator = new TrainableEvaluator();
        }

        [TestCase("N")]
        [TestCase("NV")]
        public void GivenEmptyTree_UpPatternAddsNodeWithScore_1(string pattern)
        {
            UpdateScore(pattern, 1);
            var leaf = GetLeaf(_evaluator.ScoringTree, pattern.Length);
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
            UpdateScore(pattern, 1);
            UpdateScore(pattern, -1);
            Assert.That(_evaluator.ScoringTree.Children, Is.Empty);
        }

        private void UpdateScore(string pattern, sbyte score)
            => _evaluator.UpdateScore(Encoder.Encode(pattern).ToArray(), score);
    }
}