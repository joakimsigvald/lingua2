namespace Lingua.Core
{
    public interface IEvaluation
    {
        string Fragment { get; }
        string Symbols { get; }
        int Score { get; }
    }
}