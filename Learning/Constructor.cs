using Lingua.Core;

namespace Lingua.Learning
{
    public class Constructor : ITestCaseTranslator
    {
        private readonly Translator _translator;

        public Constructor(Translator translator)
            => _translator = translator;

        public TranslationResult Translate(TestCase testCase)
            => _translator.Construct(testCase.Possibilities);
    }
}