namespace Lingua.Learning.TestCaseTranslators
{
    using Core;

    public class FullTextTranslator : ITestCaseTranslator
    {
        private readonly ITranslator _translator;

        public FullTextTranslator(ITranslator translator) => _translator = translator;

        public TranslationResult Translate(TestCase testCase)
            => _translator.Translate(testCase.From);
    }
}