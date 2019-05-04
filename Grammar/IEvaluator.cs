namespace Lingua.Grammar
{
    public interface IEvaluator
    {
        byte Horizon { get; }
        int ScorePatternsEndingWith(ushort[] invertedCode);
    }
}