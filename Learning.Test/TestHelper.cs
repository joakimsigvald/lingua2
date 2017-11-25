using System;

namespace Lingua.Learning.Test
{
    using Tokenization;
    using Core;
    using Grammar;
    using Vocabulary;

    public static class TestHelper
    {
        public static ITokenizer Tokenizer => LazyTokenizer.Value;
        public static Translator Translator => LazyTranslator.Value;
        private static TestRunner TestRunner => LazyTestRunner.Value;

        public static TestCaseResult GetTestCaseResult(string from, string expected)
            => TestRunner.RunTestCase(new TestCase(from, expected));

        private static readonly Lazy<ITokenizer> LazyTokenizer = new Lazy<ITokenizer>(CreateTokenizer);
        private static readonly Lazy<Translator> LazyTranslator = new Lazy<Translator>(CreateTranslator);
        private static readonly Lazy<TestRunner> LazyTestRunner = new Lazy<TestRunner>(CreateTestRunner);

        private static ITokenizer CreateTokenizer() => new Tokenizer();

        private static Translator CreateTranslator()
        {
            var thesaurus = new Thesaurus();
            var evaluator = new Evaluator();
            var grammar = new GrammarEngine(evaluator);
            return new Translator(Tokenizer, thesaurus, grammar);
        }

        private static TestRunner CreateTestRunner()
            => new TestRunner(Translator, Tokenizer);
    }
}