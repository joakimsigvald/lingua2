namespace Lingua.Learning.TestCaseTranslators
{
    using Core;

    public class PossibitiesTranslator : ITestCaseTranslator
    {
        private readonly Translator _translator;

        public PossibitiesTranslator(Translator translator)
            => _translator = translator;

        public TranslationResult Translate(TestCase testCase) 
            => _translator.Compose(testCase.Possibilities);
    }
}