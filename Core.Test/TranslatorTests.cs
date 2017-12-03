using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    using Learning.TestCaseTranslators;
    using Extensions;
    using Grammar;
    using Learning;
    using Tokenization;
    using Vocabulary;

    [TestFixture]
    public class TranslatorTests
    {
        private static readonly Evaluator Evaluator = GetEvaluator();
        private static readonly Translator Translator = CreateTranslator();
        private static readonly TestBench TestBench = CreateTestBench();

        private static TestBench CreateTestBench() 
            => new TestBench(new TestRunner(new FullTextTranslator(Translator)), new TestReporter());

        private static Translator CreateTranslator() 
            => new Translator(new Tokenizer(), new Thesaurus(), new GrammarEngine(Evaluator));

        private static Evaluator GetEvaluator()
        {
            var evaluator = new Evaluator();
            evaluator.Load();
            return evaluator;
        }

        [Test]
        public void TranslateNull()
            => TestCase(null, "");
                         
        [TestCase("|Bouncing ball to play with| /=> |Studsboll att leka med|")]
        [TestCase("|It is my pen| /=> |Det är min penna|")]
        [TestCase("|I am painting the wall| /=> |jag målar väggen|")]
        [TestCase("|search results| /=> |sökresultat|")]
        [TestCase("|I have been running| /=> |jag har sprungit|")]
        public void RunTestCase(string testCase)
        {
            var parts = Regex.Split(testCase, @"\s+/=>\s+");
            var from = parts[0].Trim('|');
            var to = parts[1].Trim('|');
            TestCase(from, to);
        }

        [Test]
        public void RunTestSuites()
        {
            var success = TestBench.RunTestSuites();
            Assert.That(success);
        }

        [TestCase(null, "")]
        [TestCase(".", ".")]
        public void Translate(string original, string expected)
        {
            var actual = Translator.Translate(original);
            Assert.That(actual.Translation, Is.EqualTo(expected));
        }

        private static void TestCase(string from, string to)
        {
            var result = TestBench.RunTestCase(new TestCase(from, to) {Suite = "Single"});
            if (!result.IsSuccess)
                Output(result.Reason);
            Assert.That(result.IsSuccess, $"Expected \"{result.Expected}\" but was \"{result.Actual}\"");
        }

        private static void Output(IReason reason)
            => reason.Evaluations.ForEach(Output);

        private static void Output(IEvaluation evaluation)
        {
            var symbols = evaluation.Patterns
                .Select(Encoder.Decode)
                .Select(Encoder.Serialize);
            Console.WriteLine($"{string.Join(", ", symbols)}:{evaluation.Score}");
        }
    }

    internal class TestReporter : IReporter
    {
        public void Report(TestSessionResult result)
        {
            Console.WriteLine($"Success: {result.Success}");
            Console.WriteLine($"Success count: {result.SuccessCount}");
            Console.WriteLine($"Failed case: {result.FailedCase}");
        }
    }
}