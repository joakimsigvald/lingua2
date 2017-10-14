namespace Lingua.Grammar
{
    public class Scoring
    {
        public ushort[] Pattern { get; }
        private byte Count { get; }
        private sbyte Score { get; }
        public int TotalScore => Count * Score;

        public Scoring(ushort[] pattern, byte count, sbyte score)
        {
            Pattern = pattern;
            Count = count;
            Score = score;
        }
    }
}