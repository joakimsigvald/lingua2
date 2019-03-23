namespace Lingua.Learning
{
    using Grammar;

    public interface ITrainableEvaluator {
        int ComputeScoreDeficit(TestCaseResult failedCase);
        void Do(ScoredPattern scoredPattern);
        void Undo(ScoredPattern scoredPattern);
        void Add(Arranger arranger);
        void Remove(Arranger arranger);
        sbyte GetScore(ushort[] code);
    }
}