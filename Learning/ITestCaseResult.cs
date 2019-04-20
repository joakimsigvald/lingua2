namespace Lingua.Learning
{
    using Core;

    public interface ITestCaseResult
    {
        ITranslation[] ExpectedTranslations { get; }
        ITranslation[] ActualTranslations { get; }
        IGrammaton[] ExpectedGrammatons { get; }
        IGrammaton[] ActualGrammatons { get; }
    }
}