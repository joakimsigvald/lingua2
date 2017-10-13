using Lingua.Core;

namespace Lingua.Grammar
{
    public class Scoring
    {
        public ushort[] Code { get; }
        public ushort[] Pattern { get; }
        public int Count { get; }
        private int Score { get; }
        public int TotalScore => Count * Score;

        public Scoring(ushort[] code, ushort[] pattern, int count, int score)
        {
            Code = code;
            Pattern = pattern;
            Count = count;
            Score = score;
        }
    }
}