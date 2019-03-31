using System.Collections.Generic;
using Xunit;

namespace Lingua.Grammar.Test
{
    using Core;
    using System.Linq;

    public class EvaluatorTests
    {
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
            var evaluator = Evaluator.Create(patterns);
            var reversedCode = Encoder.Encode(symbols).Reverse().ToArray();
            var actual = evaluator.ScorePatternsEndingWith(reversedCode);
            Assert.Equal(expectedScore, actual);
        }
    }
}