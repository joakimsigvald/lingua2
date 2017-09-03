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
        private readonly Modifier _modifiers;

        public WordMap()
        {
        }

        public WordMap(Modifier modifiers)
        {
            _modifiers = modifiers;
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
            var keys = from.Item1.ToArray();
            var to = VariationExpander.Expand(mapping.Value);
            var values = to.Item1.ToArray();
            var translations = keys
                .Select((key, i) => CreateTranslation(key, values[i], i))
                .ToList();
            translations[0].Variations = translations.ToArray();
            if (to.Item2 != null)
                translations.Add(CreateIncompleteCompoundTranslation(keys.First(), to.Item2));
            return translations;
        }

        private Translation CreateIncompleteCompoundTranslation(string from, string to)
        {
            var incompleteCompound = CreateTranslation(from, to, 0);
            incompleteCompound.IsIncompleteCompound = true;
            return incompleteCompound;
        }

        private Translation CreateTranslation(string from, string to, int variationIndex)
        {
            var token = new TWord
            {
                Value = from,
                Modifiers = _modifiers,
                VariationIndex = variationIndex
            };
            return Translation.Create(token, to);
        }
    }
}