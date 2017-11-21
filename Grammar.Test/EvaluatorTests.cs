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