namespace Lingua.Core
{
    public class ReductionResult
    {
        public ReductionResult()
        {
            Translations = new ITranslation[0];
            CondensedCode = new ushort[0];
        }

        public ReductionResult(ITranslation[] translations, ushort[] condensedCode, int score)
        {
            Translations = translations;
            CondensedCode = condensedCode;
            Score = score;
        }

        public ITranslation[] Translations { get; }
        public ushort[] CondensedCode { get; }
        public int Score { get; }
    }
}