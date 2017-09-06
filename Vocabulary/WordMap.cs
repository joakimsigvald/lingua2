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

        private IEnumerable<Translation> CreateTranslations(KeyValuePair<string, string> mapping)
        {
            var from = VariationExpander.Expand(mapping.Key);
            var keys = from.Variations;
            var to = VariationExpander.Expand(mapping.Value);
            var values = to.Variations;
            var translations = keys
                .Select((key, i) => CreateTranslation(key, values[i], i, from.Modifiers))
                .ToList();
            translations[0].Variations = translations.ToArray();
            if (to.IncompleteCompound != null)
                translations.Add(CreateIncompleteCompoundTranslation(keys.First(), to.IncompleteCompound, from.Modifiers));
            return translations;
        }

        private Translation CreateIncompleteCompoundTranslation(string from, string to, string modifiers)
        {
            var incompleteCompound = CreateTranslation(from, to, 0, modifiers);
            incompleteCompound.IsIncompleteCompound = true;
            return incompleteCompound;
        }

        private static Translation CreateTranslation(string from, string to, int variationIndex, string modifiers)
        {
            var token = new TWord
            {
                Value = from,
                Modifiers = Encoder.ParseModifiers(modifiers),
                VariationIndex = variationIndex
            };
            return Translation.Create(token, to);
        }
    }
}