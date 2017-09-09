using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;

namespace Lingua.Vocabulary
{
    public interface IModificationRule
    {
        Translation Apply(Translation translation);
    }

    public class ModificationRule<TWord> : IModificationRule
        where TWord : Word
    {
        private readonly Modifier _modifier;
        private readonly Transformation[] _fromTransforms;
        private readonly Transformation[] _toTransforms;

        public ModificationRule(Modifier modifier, IEnumerable<string> fromTransforms, IEnumerable<string> toTransforms)
        {
            if (modifier == Modifier.None)
                throw new ArgumentException("Rule must have modifier, was None");
            _modifier = modifier;
            _fromTransforms = Parse(fromTransforms);
            if (_fromTransforms.Any(transform => transform.To == "*"))
                throw new ArgumentException("Identity transform on from not allowed");
            _toTransforms = Parse(toTransforms);
        }

        public Translation Apply(Translation translation)
        {
            var fromWord = translation.From as TWord;
            if (fromWord == null)
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