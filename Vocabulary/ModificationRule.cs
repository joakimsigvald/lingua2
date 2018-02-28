using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;

namespace Lingua.Vocabulary
{
    public interface IModificationRule
    {
        bool AppliesTo(Type type); 
        ITranslation Apply(ITranslation translation);
        ITranslation PostApply(ITranslation translation);
    }

    public class ModificationRule : IModificationRule
    {
        private readonly Type[] _appliesTo;
        private readonly Modifier _modifier;
        private readonly Transformation[] _fromTransforms;
        private readonly Transformation[] _toTransforms;

        public ModificationRule(Type[] appliesTo, Modifier modifier, IEnumerable<string> fromTransforms, IEnumerable<string> toTransforms)
        {
            if (modifier == Modifier.None)
                throw new ArgumentException("Rule must have modifier, was None");
            _appliesTo = appliesTo;
            _modifier = modifier;
            _fromTransforms = Parse(fromTransforms);
            if (_fromTransforms.Any(transform => transform.To == "*"))
                throw new ArgumentException("Identity transform on from not allowed");
            _toTransforms = Parse(toTransforms);
        }

        public bool AppliesTo(Type type) 
            => _appliesTo.Any(t => t.IsAssignableFrom(type));

        public ITranslation Apply(ITranslation translation)
        {
            var fromWord = translation.From as Word;
            if (fromWord == null || !_appliesTo.Any(t => t.IsInstanceOfType(fromWord)))
                return null;
            var fromModification = _fromTransforms.FirstOrDefault(transform => Matches(transform.From, fromWord.Value));
            if (fromModification == null)
                return null;
            var modifiedFrom = fromWord.Clone(Modify(fromWord.Value, fromModification.To));
            modifiedFrom.Modifiers |= _modifier;
            var toModification = _toTransforms.FirstOrDefault(transform => Matches(transform.From, translation.To));
            var modifiedTo = Modify(translation.To, toModification?.To ?? "*");
            return Translation.Create(modifiedFrom, modifiedTo);
        }

        public ITranslation PostApply(ITranslation translation)
        {
            var fromWord = translation.From as Word;
            if (fromWord == null || !_appliesTo.Any(t => t.IsInstanceOfType(fromWord)))
                return null;
            var fromModification = _fromTransforms.FirstOrDefault(transform => Matches(transform.To, fromWord.Value));
            if (fromModification == null)
                return null;
            var unmodifiedFrom = fromWord.Value.Substring(0, fromWord.Value.Length - fromModification.To.Length + 1);
            fromWord.Modifiers |= _modifier;
            var toModification = _toTransforms.FirstOrDefault(transform => Matches(transform.From, unmodifiedFrom));
            var modifiedTo = Modify(unmodifiedFrom, toModification?.To ?? "*");
            return Translation.Create(fromWord, modifiedTo);
        }

        private static Transformation[] Parse(IEnumerable<string> transforms)
            => transforms
                .Select(Parse)
                .OrderByDescending(transform => transform.From.Length)
                .ToArray();

        private static Transformation Parse(string transform)
        {
            var parts = transform.Split('>');
            return new Transformation
            {
                From = parts[0],
                To = parts[1]
            };
        }

        private static string Modify(string word, string pattern)
            => word + pattern.TrimStart('*');

        private static bool Matches(string pattern, string word)
            => word.EndsWith(pattern.TrimStart('*'));
    }

    public class Transformation
    {
        public string From { get; set; }
        public string To { get; set; }
    }
}