﻿using System.Linq;
using Lingua.Core;
using Xunit;

namespace Lingua.Learning.Test
{
    using Grammar;

    public class NewTrainableEvaluatorTests
    {
        readonly NewTrainableEvaluator _evaluator;

        public NewTrainableEvaluatorTests() => _evaluator = new NewTrainableEvaluator(new Rearranger());

        [Theory]
        [InlineData("N")]
        [InlineData("NV")]
        public void GivenEmptyTree_UpPatternAddsNodeWithScore_1(string pattern)
        {
            UpdateScore(pattern, 1);
            var leaf = GetLeaf(_evaluator.Patterns, pattern.Length);
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
            Assert.Empty(_evaluator.Patterns.Previous);
        }

        private static ReverseCodeScoreNode? GetLeaf(ReverseCodeScoreNode node, int depth)
            => node == null
                ? null
                : depth <= 0
                    ? node
                    : GetLeaf(node.Previous.FirstOrDefault(), depth - 1);

        private void UpdateScore(string pattern, sbyte score)
            => _evaluator.UpdateScore(Encoder.Encode(pattern).ToArray(), score);
    }
}