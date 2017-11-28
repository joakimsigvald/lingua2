using System.Collections.Generic;

namespace Lingua.Vocabulary
{
    using Core;
    using Core.Extensions;

    public class Lexicon : ILexicon
    {
        private readonly IDictionary<string, IList<Translation>> _translations = new Dictionary<string, IList<Translation>>();

        public Lexicon(params IWordMap[] maps) => maps.ForEach(AddToDictionary);

        public IList<Translation> Lookup(string word) 
            => _translations.SafeGetValue(word) ?? new Translation[0];

        private void AddToDictionary(IWordMap map) => map.Translations.ForEach(AddToDictionary);

        private void AddToDictionary(Translation translation)
        {
            var key = translation.From.Value.Split(' ')[0];
            if (!_translations.TryGetValue(key, out IList<Translation> translations))
                _translations[key] = translations = new List<Translation>();
            translations.Add(translation);
        }
    }
}