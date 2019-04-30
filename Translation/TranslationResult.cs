using Lingua.Core;
using System.Collections.Generic;

namespace Lingua.Translation
{
    public class TranslationResult
    {
        public TranslationResult(
            string translation,
            IList<IGrammaton[]>? possibilities = null,
            ReductionResult? reduction = null,
            IGrammaton[]? arrangement = null,
            ITranslation[]? translations = null)
        {
            Translation = translation;
            Score = reduction?.Score ?? 0;
            Possibilities = possibilities ?? new List<IGrammaton[]>();
            Grammatons = reduction?.Grammatons ?? new IGrammaton[0];
            Arrangement = arrangement ?? new IGrammaton[0];
            Translations = translations ?? new ITranslation[0];
        }

        public string Translation { get; }
        public int Score { get; }
        public IList<IGrammaton[]> Possibilities { get; }
        public IGrammaton[] Grammatons { get; }
        public IGrammaton[] Arrangement { get; }
        public ITranslation[] Translations { get; }
    }
}