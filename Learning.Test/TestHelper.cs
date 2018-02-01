using System;

namespace Lingua.Learning.Test
{
    using TestCaseTranslators;
    using Tokenization;
    using Core;
    using Grammar;
    using Vocabulary;

    public static class TestHelper
    {
        public static Translator Translator => LazyTranslator.Value;
        private static ITokenizer Tokenizer => LazyTokenizer.Value;
        private static TestRunner TestRunner => LazyTestRunner.Value;
        private static TestRunner TestRunnerForAnalysis => LazyTestRunnerForAnalysis.Value;

        public static TestCaseResult GetTestCaseResult(string from, string expected)
            => TestRunner.RunTestCase(new TestCase(from, expected));

        public static TestCaseResult GetTestCaseResultForAnalysis(string from, string expected)
            => TestRunnerForAnalysis.RunTestCase(new TestCase(from, expected));

        private static readonly Lazy<ITokenizer> LazyTokenizer = new Lazy<ITokenizer>(CreateTokenizer);
        private static readonly Lazy<Translator> LazyTranslator = new Lazy<Translator>(CreateTranslator);
        private static readonly Lazy<TestRunner> LazyTestRunner = new Lazy<TestRunner>(CreateTestRunner);
        private static readonly Lazy<TestRunner> LazyTestRunnerForAnalysis = new Lazy<TestRunner>(CreateTestRunnerForAnalysis);

        private static ITokenizer CreateTokenizer() => new Tokenizer();

        private static Translator CreateTranslator()
        {
            var thesaurus = new Thesaurus();
            var evaluator = new Evaluator();
            var grammar = new GrammarEngine(evaluator);
            var capitalizer = new Capitalizer();
            evaluator.Load();
            return new Translator(Tokenizer, thesaurus, grammar, capitalizer);
        }

        private static TestRunner CreateTestRunner()
            => new TestRunner(new FullTextTranslator(Translator));

        private static TestRunner CreateTestRunnerForAnalysis()
            => new TestRunner(new FullTextTranslator(Translator)
                , settings: new TestRunnerSettings
                {
                    PrepareTestCaseForAnalysis = true
                });
    }
}