using System.Collections.Generic;
using Lingua.Core;
using NUnit.Framework;

namespace Lingua.Grammar.Test
{
    [TestFixture]
    public class EvaluatorTests
    {
        private static readonly Evaluator StoredEvaluator = new Evaluator();

        [TestCase("TN", 0)]
        [TestCase("TdqAdnNd", 1)]
        [TestCase("R1V1", 1)]
        [TestCase("R1nV1", 1)]
        [TestCase("R2V1", 1)]
        [TestCase("R2nV1", 1)]
        [TestCase("Vi", 1)]
        [TestCase("R3tX3R1mN", 1)]
        public void Score(string symbols, int expectedScore)
        {
            var code = Encoder.Encode(symbols);
            var actualScore = StoredEvaluator.Evaluate(code).Score;
            Assert.That(actualScore, Is.EqualTo(expectedScore));
        }

        [TestCase("Vi", "Vi")]
        [TestCase("^Vi", "Vi")]
        public void MatchScorer(string pattern, string symbols)
        {
            const int score = 1;
            var evaluator = new Evaluator(new Dictionary<string, int> {{pattern, score } });
            var code = Encoder.Encode(symbols);
            var actualScore = evaluator.Evaluate(code).Score;
            Assert.That(actualScore, Is.EqualTo(score));
        }
    }
}