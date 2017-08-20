using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Lingua.Core;
using Lingua.Core.Tokens;

namespace Lingua.Vocabulary
{
    public interface IWordMap
    {
        IEnumerable<Translation> Translations { get; }
    }

    [Serializable]
    public class WordMap<TWord> : Dictionary<string, string>, IWordMap
        where TWord : Word, new()
    {
        public WordMap()
        {
        }

        public WordMap(IDictionary<string, string> mappings)
            : base(mappings)
        {
        }

        protected WordMap(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IEnumerable<Translation> Translations => this.SelectMany(CreateTranslations);

        private static IEnumerable<Translation> CreateTranslations(KeyValuePair<string, string> mapping)
        {
            var keys = VariationExpander.Expand(mapping.Key).ToArray();
            var values = VariationExpander.Expand(mapping.Value).ToArray();
            var translations = keys
                .Select((key, i) => CreateTranslation(key, values[i], i))
                .ToArray();
            translations[0].Variations = translations;
            return translations;
        }

        private static Translation CreateTranslation(string from, string to, int variationIndex)
        {
            var token = new TWord {Value = from, VariationIndex = variationIndex };
            return Translation.Create(token, to);
        }
    }
}