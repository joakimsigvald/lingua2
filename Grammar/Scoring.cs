using Lingua.Core;

namespace Lingua.Grammar
{
    public class Scoring
    {
        public string Pattern { get; }
        public int Count { get; }
        private int Score { get; }
        public int TotalScore => Count * Score;

        public Scoring(int[] pattern, int count, int score)
        {
            Pattern = Encoder.Serialize(Encoder.Decode(pattern));
            Count = count;
            Score = score;
        }
    }
}