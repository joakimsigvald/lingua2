using System.Collections.Generic;
using Xunit;

namespace Lingua.Grammar.Test
{
    using Core;
    using System.Linq;

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

        [Fact]
        public void EmptyPattern_GiveScore_Zero()
        {
            var evaluator = new NewEvaluator();
            var actual = evaluator.EvaluateReversed(new ushort[0]);
            Assert.Equal(0, actual);
        }

        [Theory]
        [InlineData("Vi", "Vi")]
        [InlineData("ATN", "ATN")]
        [InlineData("^ATN", "^ATN")]
        public void OneFullMatch_GiveScoreOfMatchedPattern(string pattern, string symbols)
            => VerifyScoreOfMatchedPatterns(symbols, 1, $"{pattern}:1");

        [Theory]
        [InlineData("A_N", "ATN")]
        public void OneFullMatchWithWildcard_GiveScoreOfMatchedPattern(string pattern, string symbols)
            => VerifyScoreOfMatchedPatterns(symbols, 1, $"{pattern}:1");

        [Theory]
        [InlineData("ATN", 2, "ATN:2", "Vi:3")]
        public void OneFullMatchOutOfMany_GiveScoreOfMatchedPattern(string symbols, sbyte expectedScore, params string[] scoredPatterns)
            => VerifyScoreOfMatchedPatterns(symbols, expectedScore, scoredPatterns);

        [Theory]
        [InlineData("VATN", 2, "ATN:2")]
        public void LongerSequenceEndingWithPattern_GiveScoreOfMatchedPattern(string symbols, sbyte expectedScore, params string[] scoredPatterns)
            => VerifyScoreOfMatchedPatterns(symbols, expectedScore, scoredPatterns);

        [Theory]
        [InlineData("VATN", 6, "TVATN:4", "VATN:3", "ATN:2", "TN:1")]
        public void SequenceMatchingMultiplePatterns_GiveSumOfScoresOfMatchedPatterns(string symbols, sbyte expectedScore, params string[] scoredPatterns)
            => VerifyScoreOfMatchedPatterns(symbols, expectedScore, scoredPatterns);

        private void VerifyScoreOfMatchedPatterns(string symbols, sbyte expectedScore, params string[] scoredPatterns)
        {
            var patterns = scoredPatterns.Select(sp => sp.Split(':')).ToDictionary(parts => parts[0], parts => sbyte.Parse(parts[1]));
            var evaluator = new NewEvaluator(patterns);
            var reversedCode = Encoder.Encode(symbols).Reverse().ToArray();
            var actual = evaluator.EvaluateReversed(reversedCode);
            Assert.Equal(expectedScore, actual);
        }
    }
}