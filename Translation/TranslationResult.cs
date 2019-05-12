using Lingua.Core;

namespace Lingua.Translation
{
    public class TranslationResult
    {
        public static readonly TranslationResult Empty = new TranslationResult(
            string.Empty,
            new Decomposition(new[] { new ITranslation[0]} ),
            ReductionResult.Empty,
            new IGrammaton[0],
            new ITranslation[0]);

        public TranslationResult(
            string translation,
            IDecomposition decomposition,
            ReductionResult reduction,
            IGrammaton[] arrangement,
            ITranslation[] translations)
        {
            Translation = translation;
            Score = reduction.Score;
            Decomposition = decomposition;
            Grammatons = reduction.Grammatons;
            Arrangement = arrangement;
            Translations = translations;
        }

        public string Translation { get; }
        public int Score { get; }
        public IDecomposition Decomposition { get; }
        public IGrammaton[] Grammatons { get; }
        public IGrammaton[] Arrangement { get; }
        public ITranslation[] Translations { get; }
    }
}