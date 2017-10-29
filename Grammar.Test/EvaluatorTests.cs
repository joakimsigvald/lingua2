using System.Collections.Generic;
using NUnit.Framework;

namespace Lingua.Grammar.Test
{
    using Core;

    [TestFixture]
    public class EvaluatorTests
    {
        private static Evaluator _storedEvaluator;

        [OneTimeSetUp]
        public void SetUp()
        {
            _storedEvaluator = new Evaluator();
            _storedEvaluator.Load();
        }

        [TestCase("^TN", 0)]
        [TestCase("^TdqAdnNd", 1)]
        [TestCase("^R1V1", 1)]
        [TestCase("^R1nV1", 1)]
        [TestCase("^R2V1", 1)]
        [TestCase("^R2nV1", 1)]
        [TestCase("^Vi", 1)]
        [TestCase("^R3tX3R1mN", 1)]
        public void Score(string symbols, int expectedScore)
        {
            var code = Encoder.Encode(symbols);
            var actualScore = _storedEvaluator.Evaluate(code).Score;
            Assert.That(actualScore, Is.EqualTo(expectedScore));
        }

        [TestCase("Vi", "^Vi")]
        [TestCase("^Vi", "^Vi")]
        public void MatchScorer(string pattern, string symbols)
        {
            const sbyte score = 1;
            var evaluator = new Evaluator(new Dictionary<string, sbyte> {{pattern, score } });
            var code = Encoder.Encode(symbols);
            var actualScore = evaluator.Evaluate(code).Score;
            Assert.That(actualScore, Is.EqualTo(score));
        }
    }
}