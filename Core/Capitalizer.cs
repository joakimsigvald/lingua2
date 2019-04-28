using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    public class Capitalizer : ICapitalizer
    {
        public IEnumerable<ITranslation> Capitalize(
            ITranslation[] arrangedTranslations,
            IGrammaton[] original)
        {
            var sentences = GetSentences(arrangedTranslations, original);
            return sentences.SelectMany(s => new CapitalizationProcess(s).Capitalize());
        }

        private IEnumerable<Sentence> GetSentences(ITranslation[] arrangedTranslations, IGrammaton[] original)
        {
            if (!arrangedTranslations.Any())
                yield break;
            var remaining = original.ToList();
            var next = new List<ITranslation>();
            var nextIsCapitalized = IsCapitalized(remaining[0]);
            foreach (var t in arrangedTranslations.Where(t => !string.IsNullOrEmpty(t.Output)))
            {
                next.Add(t);
                var startOfNextSentence = GetIndex(remaining, t);
                remaining.RemoveAt(startOfNextSentence);
                if (IsEndOfSentence(t))
                {
                    yield return new Sentence(nextIsCapitalized ?? true, next.ToArray());
                    next = new List<ITranslation>();
                    if (remaining.Count == startOfNextSentence)
                        yield break;
                    nextIsCapitalized = IsCapitalized(remaining[startOfNextSentence]);
                }
            }
            if (next.Any())
                yield return new Sentence(nextIsCapitalized ?? false, next.ToArray());
        }

        private int GetIndex(List<IGrammaton> grammatons, ITranslation translation)
        {
            for (var i = 0; i < grammatons.Count; i++)
                if (grammatons[i].Translations.Contains(translation))
                    return i;
            return -1;
        }

        private bool? IsCapitalized(IGrammaton t)
            => t.IsCapitalized ? true
            : char.IsUpper(t.Input.FirstOrDefault()) ? (bool?)null : false;

        private bool IsEndOfSentence(ITranslation t) => t.From is Terminator || t.From is Ellipsis;
    }

    public class Sentence
    {
        public Sentence(bool isCapitalized, ITranslation[] translations)
        {
            IsCapitalized = isCapitalized;
            Translations = translations;
        }

        public bool IsCapitalized { get; } 
        public ITranslation[] Translations { get; }
    }

    public class CapitalizationProcess
    {
        private readonly Sentence _sentence;

        public CapitalizationProcess(Sentence sentence) => _sentence = sentence;

        public IEnumerable<ITranslation> Capitalize()
        {
            var decapitalized = Recapitalize(_sentence.Translations);
            return _sentence.IsCapitalized
                ? decapitalized.Skip(1).Prepend(decapitalized.First().Capitalize())
                : decapitalized;
        }

        public IEnumerable<ITranslation> Recapitalize(ITranslation[] translations)
            => translations.Select(
                (t, i) => IsName(t, translations[..i])
                ? GetCapitalized(t)
                : GetDecapitalized(t));

        private ITranslation GetDecapitalized(ITranslation t)
            => t.IsCapitalized ? t.Decapitalize() : t;

        private ITranslation GetCapitalized(ITranslation t)
            => t.IsCapitalized ? t : t.Capitalize();

        private bool IsName(ITranslation t, ITranslation[] previous)
            => IsNamedEntity(t) || IsNamedNoun(t, previous);

        private bool IsNamedEntity(ITranslation t)
            => t.From is Name || t.From is Unclassified && t.IsCapitalized;

        private bool IsNamedNoun(ITranslation t, ITranslation[] previous)
            => previous.Any() && IsNamedEntity(previous[^1]) && t.From is Noun && t.IsCapitalized;
    }
}