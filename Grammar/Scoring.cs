using Lingua.Core;

namespace Lingua.Grammar
{
    public class Scoring
    {
        public ushort[] Pattern { get; }
        public int Count { get; }
        private int Score { get; }
        public int TotalScore => Count * Score;

        public Scoring(ushort[] pattern, int count, int score)
        {
            Pattern = pattern;
            Count = count;
            Score = score;
        }
    }
}