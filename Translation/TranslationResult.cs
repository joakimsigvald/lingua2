using Lingua.Core;
using System.Collections.Generic;

namespace Lingua.Translation
{
    public class TranslationResult
    {
        public static readonly TranslationResult Empty = new TranslationResult(
            string.Empty,
            new List<IGrammaton[]>(),
            ReductionResult.Empty,
            new IGrammaton[0],
            new ITranslation[0]);

        public TranslationResult(
            string translation,
            IList<IGrammaton[]> possibilities,
            ReductionResult reduction,
            IGrammaton[] arrangement,
            ITranslation[] translations)
        {
            Translation = translation;
            Score = reduction.Score;
            Possibilities = possibilities;
            Grammatons = reduction.Grammatons;
            Arrangement = arrangement;
            Translations = translations;
        }

        public string Translation { get; }
        public int Score { get; }
        public IList<IGrammaton[]> Possibilities { get; }
        public IGrammaton[] Grammatons { get; }
        public IGrammaton[] Arrangement { get; }
        public ITranslation[] Translations { get; }
    }
}