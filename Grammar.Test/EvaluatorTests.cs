using System.Collections.Generic;
using Xunit;

namespace Lingua.Grammar.Test
{
    using Core;

    public class EvaluatorTests
    {
        [Theory]
        [InlineData("Vi", "^Vi", 1)]
        [InlineData("^Vi", "^Vi", 1)]
        [InlineData("^Vi", "Vi", 0)]
        [InlineData("A_N", "ATN", 1)]
        public void MatchScorer(string pattern, string symbols, int expected)
        {
            const sbyte score = 1;
            var evaluator = new Evaluator(new Dictionary<string, sbyte> {{pattern, score } });
            var code = Encoder.Encode(symbols);
            var actualScore = evaluator.Evaluate(code).Score;
            Assert.Equal(expected, actualScore);
        }
    }
}