namespace Lingua.Learning.TestCaseTranslators
{
    using Core;

    public interface ITestCaseTranslator
    {
        TranslationResult Translate(TestCase testCase);
    }
}