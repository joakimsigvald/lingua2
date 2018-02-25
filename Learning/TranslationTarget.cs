namespace Lingua.Learning
{
    using Core;
    using Grammar;

    public class TranslationTarget
    {
        public ITranslation[] Translations { get; set; }
        public Arrangement Arrangement { get; set; }
        public string Unmatched { get; set; }
    }
}