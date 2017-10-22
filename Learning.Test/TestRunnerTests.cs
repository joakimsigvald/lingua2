using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    using Core;
    using Grammar;
    using Testing;
    using Tokenization;
    using Vocabulary;

    [TestFixture]
    public class TestRunnerTests
    {
        [TestCase("I run", "jag springer", "R1V1")]
        [TestCase("2 [[ball]]", "2 bollar", "QnNn")]
        public void MatchesPositivePatterns(string from, string expected, params string[] expectedPatterns)
        {
            var result = GetTestCaseResult(from, expected);
            var positivePatterns = PatternGenerator.GetMatchingPatterns(result)
                .Where(sp => sp.Item2 > 0)
                .Select(sp => sp.Item1);
            Assert.That(positivePatterns.Intersect(expectedPatterns), Is.EquivalentTo(expectedPatterns));
        }

        [TestCase("search result", "sökresultat", "NN")]
        public void MatchesNegativePatterns(string from, string expected, params string[] expectedPatterns)
        {
            var result = GetTestCaseResult(from, expected);
            var positivePatterns = PatternGenerator.GetMatchingPatterns(result)
                .Where(sp => sp.Item2 < 0)
                .Select(sp => sp.Item1);
            Assert.That(positivePatterns.Intersect(expectedPatterns), Is.EquivalentTo(expectedPatterns));
        }

        private static TestCaseResult GetTestCaseResult(string from, string expected)
        {
            var testCase = new TestCase
            {
                From = from,
                Expected = expected
            };
            var tokenizer = new Tokenizer();
            var thesaurus = new Thesaurus();
            var evaluator = new Evaluator(new Dictionary<string, sbyte>());
            var engine = new Engine(evaluator);
            var translator = new Translator(tokenizer, thesaurus, engine);
            var testRunner = new TestRunner(translator, tokenizer);
            return testRunner.RunTestCase(testCase);
        }
    }
}