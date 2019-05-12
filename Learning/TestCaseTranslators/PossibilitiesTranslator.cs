namespace Lingua.Learning.TestCaseTranslators
{
    using Core;
    using Lingua.Translation;

    public class PossibitiesTranslator : ITestCaseTranslator
    {
        private readonly Translator _translator;

        public PossibitiesTranslator(Translator translator)
            => _translator = translator;

        public TranslationResult Translate(TestCase testCase)
            => testCase.Result ?? (testCase.Result = DoTranslate(testCase));

        private TranslationResult DoTranslate(TestCase testCase)
            => _translator.Arrange(testCase.Decomposition, GetReduction(testCase));

        private ReductionResult GetReduction(TestCase testCase)
            => testCase.Reduction ?? (testCase.Reduction = _translator.Reduce(testCase.Decomposition));
    }
}