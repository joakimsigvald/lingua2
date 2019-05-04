namespace Lingua.Learning
{
    using Grammar;

    public interface ITrainableEvaluator {
        int ComputeScoreDeficit(TestCaseResult failedCase);
        void Do(ScoredPattern scoredPattern);
        void Undo(ScoredPattern scoredPattern);
        void Add(Arrangement arranger);
        void Remove(Arrangement arranger);
        sbyte GetScore(ushort[] reversedCode);
    }
}