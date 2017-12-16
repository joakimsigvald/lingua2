using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Lingua.Vocabulary
{
    using Core;
    using Core.Extensions;
    using Core.Tokens;

    public interface IWordMap
    {
        IEnumerable<Translation> Translations { get; }
    }

    [Serializable]
    public class WordMap<TWord> : Dictionary<string, string>, IWordMap
        where TWord : Word, new()
    {
        private readonly IList<IModificationRule> _rules = new List<IModificationRule>();
        private readonly int _baseForm;

        public WordMap(IList<IModificationRule> rules = null, int baseForm = 0)
        {
            _rules = rules ?? _rules;
            _baseForm = baseForm;
        }

        public WordMap(IDictionary<string, string> mappings, IList<IModificationRule> rules = null, int baseForm = 0)
            : base(mappings)
        {
            _rules = rules ?? _rules;
            _baseForm = baseForm;
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
            var to = VariationExpander.Expand(mapping.Value);
            var baseTranslations = from.Variations
                .Select((key, i) => CreateTranslation(key, to.Variations[i], from.Modifiers, i))
                .ToList();
            var allTranslations = baseTranslations.Concat(ApplyRules(baseTranslations)).ToList();
            allTranslations[0].Variations = allTranslations.ToArray();
            if (to.IncompleteCompound != null)
                allTranslations.Add(CreateIncompleteCompoundTranslation(from, to));
            return allTranslations
                .Take(_baseForm)
                .Concat(allTranslations.Skip(_baseForm + 1))
                .Prepend(allTranslations[_baseForm])
                .Where(t => !string.IsNullOrEmpty(t.From.Value));
        }

        private IEnumerable<Translation> ApplyRules(IEnumerable<Translation> translations)
            => translations.SelectMany(ApplyRules).ExceptNull();

        private IEnumerable<Translation> ApplyRules(Translation translation)
            => _rules.Select(rule => rule.Apply(translation));

        private Translation CreateIncompleteCompoundTranslation(Specification from, Specification to)
        {
            var incompleteCompound = CreateTranslation(from.Base, to.IncompleteCompound, from.Modifiers);
            incompleteCompound.IsIncompleteCompound = true;
            return incompleteCompound;
        }

        private static Translation CreateTranslation(string from, string to, string modifiers, int variationIndex = 0)
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