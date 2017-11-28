using Lingua.Core;

namespace Lingua.Learning
{
    public class WordByWordTranslator : ITestCaseTranslator
    {
        private readonly IGrammar _grammar;

        public WordByWordTranslator(IGrammar grammar)
            => _grammar = grammar;

        public TranslationResult Translate(TestCase testCase)
        {
            (var translations, var reason) = _grammar.Reduce(testCase.Possibilities);
            return new TranslationResult
            {
                Translations = translations,
                Reason = reason,
                Possibilities = testCase.Possibilities
            };
        }
    }
}