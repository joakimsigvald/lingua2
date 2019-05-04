namespace Lingua.Core
{
    public class ReductionResult
    {
        public static readonly ReductionResult Empty 
            = new ReductionResult(new IGrammaton[0], new ushort[0], 0);

        public ReductionResult(IGrammaton[] grammatons, ushort[] condensedCode, int score)
        {
            Grammatons = grammatons;
            CondensedCode = condensedCode;
            Score = score;
        }

        public IGrammaton[] Grammatons { get; }
        public ushort[] CondensedCode { get; }
        public int Score { get; }
    }
}