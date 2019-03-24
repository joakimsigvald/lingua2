namespace Lingua.Learning
{
    using Core;
    using Grammar;
    using System.Linq;

    public class TranslationTarget
    {
        public TranslationTarget(Arrangement arrangement, string unmatched, ITranslation[] translations)
        {
            Arrangement = arrangement;
            Unmatched = unmatched;
            Translations = translations;
            ReversedCode = Encoder.Encode(translations).Reverse().ToArray();
        }

        public ushort[] ReversedCode { get; }
        public ITranslation[] Translations { get; }
        public Arrangement Arrangement { get; }
        public string Unmatched { get; }
    }
}