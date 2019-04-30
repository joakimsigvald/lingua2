namespace Lingua.Learning.TestCaseTranslators
{
    using Core;
    using Lingua.Translation;

    public interface ITestCaseTranslator
    {
        TranslationResult Translate(TestCase testCase);
    }
}