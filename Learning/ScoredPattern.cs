namespace Lingua.Learning
{
    public class ScoredPattern
    {
        public ScoredPattern(string pattern, sbyte score)
        {
            Pattern = pattern;
            Score = score;
        }

        public string Pattern { get; }
        public sbyte Score { get; }
    }
}