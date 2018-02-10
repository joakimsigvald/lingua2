using System.Collections.Generic;

namespace Lingua.Vocabulary
{
    using Core;
    using Core.Extensions;

    public class Lexicon : ILexicon
    {
        private readonly IDictionary<string, IList<ITranslation>> _translations = new Dictionary<string, IList<ITranslation>>();

        public Lexicon(params IWordMap[] maps) => maps.ForEach(AddToDictionary);

        public IList<ITranslation> Lookup(string word) 
            => _translations.SafeGetValue(word) ?? new ITranslation[0];

        private void AddToDictionary(IWordMap map) => map.Translations.ForEach(AddToDictionary);

        private void AddToDictionary(ITranslation translation)
        {
            var key = translation.From.Value.Split(' ')[0];
            if (!_translations.TryGetValue(key, out IList<ITranslation> translations))
                _translations[key] = translations = new List<ITranslation>();
            translations.Add(translation);
        }
    }
}