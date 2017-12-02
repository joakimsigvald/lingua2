namespace Lingua.Learning.TestCaseTranslators
{
    using Core;

    public class FullTextTranslator : ITestCaseTranslator
    {
        private readonly ITranslator _translator;

        public FullTextTranslator(ITranslator translator) => _translator = translator;

        public TranslationResult Translate(TestCase testCase)
        {
            var result = _translator.Translate(testCase.From);
            AssureTargetSet(testCase, result);
            return result;
        }

        private static void AssureTargetSet(TestCase testCase, TranslationResult translationResult)
        {
            if (testCase.Target != null)
                return;
            testCase.Target = testCase.Target ?? TargetSelector
                                  .SelectTarget(translationResult.Possibilities, testCase.Expected);
        }
    }
}