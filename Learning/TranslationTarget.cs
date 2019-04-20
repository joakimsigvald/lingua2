namespace Lingua.Learning
{
    using Core;
    using Grammar;
    using System.Collections.Generic;
    using System.Linq;

    public class TranslationTarget
    {
        public TranslationTarget(Arrangement arrangement, ITranslation[] translations)
        {
            Arrangement = arrangement;
            Translations = translations;
            Grammatons = translations.Select(t => new Grammaton(t)).ToArray();
            ExpectedPossibilities = translations.Select(t => new[] { t }).ToArray();
        }

        public ITranslation[] Translations { get; }
        public IGrammaton[] Grammatons { get; }
        public IList<ITranslation[]> ExpectedPossibilities { get; }
        public Arrangement Arrangement { get; }
    }
}