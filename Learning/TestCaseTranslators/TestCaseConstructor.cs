namespace Lingua.Learning.TestCaseTranslators
{
    using Core;

    public class TestCaseConstructor : ITestCaseTranslator
    {
        private readonly Translator _translator;

        public TestCaseConstructor(Translator translator)
            => _translator = translator;

        public TranslationResult Translate(TestCase testCase)
            => _translator.Compose(testCase.Possibilities);
    }
}