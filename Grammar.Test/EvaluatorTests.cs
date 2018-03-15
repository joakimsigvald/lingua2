using System.Collections.Generic;
using NUnit.Framework;

namespace Lingua.Grammar.Test
{
    using Core;

    [TestFixture]
    public class EvaluatorTests
    {
        [TestCase("Vi", "^Vi", 1)]
        [TestCase("^Vi", "^Vi", 1)]
        [TestCase("^Vi", "Vi", 0)]
        [TestCase("A_N", "ATN", 1)]
        public void MatchScorer(string pattern, string symbols, int matchCount)
        {
            const sbyte score = 1;
            var evaluator = new Evaluator(new Dictionary<string, sbyte> {{pattern, score } });
            var code = Encoder.Encode(symbols);
            var actualScore = evaluator.Evaluate(code).Score;
            Assert.That(actualScore, Is.EqualTo(matchCount));
        }
    }
}