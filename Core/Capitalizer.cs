using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    public class Capitalizer : ICapitalizer
    {
        public IEnumerable<ITranslation> Capitalize(
            ITranslation[] arrangedTranslations,
            ITranslation[] allTranslations)
        {
            var sentences = GetSentences(arrangedTranslations, allTranslations);
            return sentences.SelectMany(s => new CapitalizationProcess(s).Capitalize());
        }

        private IEnumerable<Sentence> GetSentences(ITranslation[] arrangedTranslations, ITranslation[] allTranslations)
        {
            if (!arrangedTranslations.Any())
                yield break;
            var remaining = allTranslations.ToList();
            var next = new List<ITranslation>();
            var nextIsCapitalized = IsCapitalized(remaining[0]);
            foreach (var t in arrangedTranslations.Where(t => !string.IsNullOrEmpty(t.Output)))
            {
                next.Add(t);
                var startOfNextSentence = remaining.IndexOf(t);
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

        private bool? IsCapitalized(ITranslation t)
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
            var decapitalized = _sentence.Translations.Select(
                t => IsName(t)
                ? GetCapitalized(t)
                : GetDecapitalized(t));
            return _sentence.IsCapitalized
                ? decapitalized.Skip(1).Prepend(decapitalized.First().Capitalize())
                : decapitalized;
        }

        private ITranslation GetDecapitalized(ITranslation t)
            => t.IsCapitalized ? t.Decapitalize() : t;

        private ITranslation GetCapitalized(ITranslation t)
            => t.IsCapitalized ? t : t.Capitalize();

        private bool IsName(ITranslation t)
            => t.From is Unclassified && t.IsCapitalized;
    }
}