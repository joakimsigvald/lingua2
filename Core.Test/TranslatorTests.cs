using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    using Grammar;
    using Learning;
    using Tokenization;
    using Vocabulary;

    [TestFixture]
    public class TranslatorTests
    {
        private static readonly TestBench TestBench = CreateTestBench();

        private static TestBench CreateTestBench()
        {
            var reporter = new FakeReporter();
            var evaluator = new Evaluator();
            evaluator.Load();
            var engine = new Engine(evaluator);
            var tokenizer = new Tokenizer();
            var translator = new Translator(new Tokenizer(), new Thesaurus(), engine);
            var testRunner = new TestRunner(translator, tokenizer);
            return new TestBench(testRunner, reporter);
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

        private static void TestCase(string from, string to)
        {
            var result = TestBench.RunTestCase(new TestCase(from, to) {Suite = "Single"});
            if (!result.Success)
                Output(result.Reason);
            Assert.That(result.Success, $"Expected \"{result.Expected}\" but was \"{result.Actual}\"");
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

    internal class FakeReporter : IReporter
    {
        public void Report(TestSessionResult result)
        {
            // Nope
        }
    }
}