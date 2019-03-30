using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    public class TranslationResult
    {
        public TranslationResult(
            string translation,
            ITranslation[]? translations = null,
            IList<ITranslation[]>? possibilities = null)
        {
            Translation = translation;
            Translations = translations ?? new ITranslation[0];
            Possibilities = possibilities ?? new List<ITranslation[]>();
            ReversedCode = Encoder.Encode(Translations).Reverse().ToArray();
        }

        public string Translation { get; }
        public ITranslation[] Translations { get; }
        public IList<ITranslation[]> Possibilities { get; }
        public ushort[] ReversedCode { get; }
    }
}