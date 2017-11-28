using Lingua.Core;

namespace Lingua.Learning
{
    public interface ITestCaseTranslator
    {
        TranslationResult Translate(TestCase testCase);
    }
}