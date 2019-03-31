using System.Collections.Generic;

namespace Lingua.Core
{
    public class TranslationResult
    {
        public TranslationResult(
            string translation,
            ReductionResult? reduction = null,
            IList<ITranslation[]>? possibilities = null)
        {
            Translation = translation;
            Reduction = reduction ?? new ReductionResult();
            Possibilities = possibilities ?? new List<ITranslation[]>();
        }

        public string Translation { get; }
        public ReductionResult Reduction { get; }
        public IList<ITranslation[]> Possibilities { get; }
    }
}