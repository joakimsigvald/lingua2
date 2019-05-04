namespace Lingua.Grammar
{
    public interface IEvaluator
    {
        byte Horizon { get; }
        ReverseCodeScoreNode Patterns { get; }
        int ScorePatternsEndingWith(ushort[] invertedCode);
    }
}