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
            var from = VariationExpander.Expand(mapping.Key);
            var keys = from.Item1.ToArray();
            var to = VariationExpander.Expand(mapping.Value);
            var values = to.Item1.ToArray();
            var translations = keys
                .Select((key, i) => CreateTranslation(key, values[i], i))
                .ToList();
            translations[0].Variations = translations.ToArray();
            if (to.Item2 != null)
            {
                var incompleteCompound = CreateTranslation(keys.First(), to.Item2, 0);
                incompleteCompound.IsIncompleteCompound = true;
                translations.Add(incompleteCompound);
            }
            return translations;
        }

        private static Translation CreateTranslation(string from, string to, int variationIndex)
        {
            var token = new TWord {Value = from, VariationIndex = variationIndex };
            return Translation.Create(token, to);
        }
    }
}