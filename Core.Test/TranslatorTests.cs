using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Lingua.Core.Test
{
    using Learning.TestCaseTranslators;
    using Extensions;
    using Grammar;
    using Learning;
    using Tokenization;
    using Vocabulary;

    public class TranslatorTests
    {
        private static readonly IEvaluator Evaluator = Grammar.Evaluator.Load();
        private static readonly IArranger Arranger = GetArranger();
        private static readonly Translator Translator = CreateTranslator();
        private static readonly TestBench TestBench = CreateTestBench();

        private static TestBench CreateTestBench() 
            => new TestBench(new TestRunner(new FullTextTranslator(Translator)), new TestReporter());

        private static Translator CreateTranslator() 
            => new Translator(new Tokenizer(), new Thesaurus(), new GrammarEngine(Evaluator), Arranger, new Capitalizer());

        private static IArranger GetArranger()
        {
            var arranger = new Rearranger();
            arranger.Load();
            return arranger;
        }

        [Fact]
        public void TranslateNull()
            => TestCase(null, "");

        [Theory]
        [InlineData("|Joakim's| /=> |Joakims|")]
        [InlineData("|I am here| /=> |jag är här|")]
        [InlineData("|Bouncing ball to play with| /=> |Studsboll att leka med|")]
        [InlineData("|It is my pen| /=> |Det är min penna|")]
        [InlineData("|I am painting the wall| /=> |jag målar väggen|")]
        [InlineData("|search results| /=> |sökresultat|")]
        [InlineData("|I have been running| /=> |jag har sprungit|")]
        [InlineData("|The rat made a nest and slept in it.| /=> |Råttan gjorde ett bo och sov i det.|")]
        [InlineData("|The red ball.| /=> |Den röda bollen.|")]
        public void RunTestCase(string testCase)
        {
            var parts = Regex.Split(testCase, @"\s+/=>\s+");
            var from = parts[0].Trim('|');
            var to = parts[1].Trim('|');
            TestCase(from, to);
        }

        [Trait("Category", "Longrunning")]
        [Fact(Skip = "Regenerate patterns")]
        public void RunTestSuites()
        {
            var success = TestBench.RunTestSuites();
            Assert.True(success);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData(".", ".")]
        public void Translate(string original, string expected)
        {
            var actual = Translator.Translate(original);
            Assert.Equal(expected, actual.Translation);
        }

        private static void TestCase(string from, string to)
        {
            var result = TestBench.RunTestCase(new TestCase(from, to) {Suite = "Single"});
            Assert.True(result.Success, $"Expected \"{result.Expected}\" but was \"{result.Actual}\"");
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