using System.Collections.Generic;

namespace Lingua.Core
{
    public class TranslationResult
    {
        public TranslationResult(
            string translation,
            ReductionResult? reduction = null,
            ITranslation[]? translations = null,
            IList<IGrammaton[]>? possibilities = null)
        {
            Translation = translation;
            Score = reduction?.Score ?? 0;
            Grammatons = reduction?.Grammatons ?? new IGrammaton[0];
            Translations = translations ?? new ITranslation[0];
            Possibilities = possibilities ?? new List<IGrammaton[]>();
        }

        public string Translation { get; }
        public int Score { get; }
        public IList<IGrammaton[]> Possibilities { get; }
        public ITranslation[] Translations { get; }
        public IGrammaton[] Grammatons { get; }
    }
}