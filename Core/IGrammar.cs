namespace Lingua.Core
{
    public interface IGrammar
    {
        ReductionResult Reduce(IDecomposition decomposition);
        ReductionResult Evaluate(IGrammaton[] grammatons);
    }
}