using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Grammar;
using Lingua.Testing;
using Lingua.Tokenization;
using Lingua.Vocabulary;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class TestRunnerTests
    {
        [TestCase("I run", "jag springer", "R1V1")]
        public void Test(string from, string expected, params string[] expectedPatterns)
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
            var result = testRunner.RunTestCase(testCase);
            var matchingPatterns = PatternGenerator.GetMatchingPatterns(result);
            Assert.That(matchingPatterns.Intersect(expectedPatterns), Is.EquivalentTo(expectedPatterns));
        }
    }
}
