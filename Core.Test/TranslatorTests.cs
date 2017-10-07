using System;
using System.Text.RegularExpressions;
using Lingua.Grammar;
using Lingua.Testing;
using Lingua.Tokenization;
using Lingua.Vocabulary;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    [TestFixture]
    public class TranslatorTests
    {
        private static readonly TestBench TestBench = new TestBench(
            new Translator(new Tokenizer(), new Thesaurus(), new Engine()));

        [Test]
        public void TranslateNull()
            => TestCase(null, "");
                         
        [TestCase("|Bouncing ball to play with| /=> |Studsboll att leka med|")]
        [TestCase("|It is my pen| /=> |Det är min penna|")]
        [TestCase("|I am painting the wall| /=> |jag målar väggen|")]
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
            var result = TestBench.RunTestCase(from, to);
            if (!result.Success)
                Output(result.Reason);
            Assert.That(result.Success, $"Expected \"{result.Expected}\" but was \"{result.Actual}\"");
        }

        private static void Output(IReason reason)
            => reason.Evaluations.ForEach(Output);

        private static void Output(IEvaluation evaluation)
            => Console.WriteLine($"{evaluation.Fragment}:{evaluation.Symbols}:{evaluation.Score}");
    }
}