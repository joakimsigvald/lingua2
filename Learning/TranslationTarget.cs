namespace Lingua.Learning
{
    using Core;
    using Grammar;
    using Lingua.Translation;
    using System.Linq;

    public class TranslationTarget
    {
        public TranslationTarget(Arrangement arrangement, ITranslation[] translations)
        {
            Arrangement = arrangement;
            Translations = translations;
            Grammatons = translations.Select(t => new Grammaton(t)).ToArray();
        }

        public ITranslation[] Translations { get; }
        public IGrammaton[] Grammatons { get; }
        public Arrangement Arrangement { get; }
    }
}