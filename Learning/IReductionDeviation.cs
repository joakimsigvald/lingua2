namespace Lingua.Learning
{
    using Core;

    public interface IReductionDeviation
    {
        IGrammaton[] ExpectedGrammatons { get; }
        IGrammaton[] ActualGrammatons { get; }
    }
}