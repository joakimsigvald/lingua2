using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Extensions;
using Lingua.Core.Tokens;

namespace Lingua.Core
{
    public class Capitalizer : ICapitalizer
    {
        public IEnumerable<ITranslation> Capitalize(IList<ITranslation> arrangedTranslations, IList<ITranslation> allTranslations)
        {
            var recapitalized = PromoteInvisibleCapitalizations(arrangedTranslations, allTranslations);
            return CapitalizeStartOfSentences(recapitalized);
        }

        private static IEnumerable<ITranslation> CapitalizeStartOfSentences(IEnumerable<ITranslation> translations)
            => SeparateSentences(translations).SelectMany(CapitalizeStartOfSentence);

        private static IEnumerable<IList<ITranslation>> SeparateSentences(IEnumerable<ITranslation> translations)
        {
            var nextSequence = new List<ITranslation>();
            foreach (var translation in translations)
            {
                nextSequence.Add(translation);
                if (!IsEndOfSentence(translation.From)) continue;
                yield return nextSequence;
                nextSequence = new List<ITranslation>();
            }
            yield return nextSequence;
        }

        private static IEnumerable<ITranslation> CapitalizeStartOfSentence(IList<ITranslation> sequence)
        {
            if (!IsSentence(sequence))
                return sequence;
            var preWord = sequence.TakeWhile(t => !(t.From is Element)).ToArray();
            var sentence = sequence.Skip(preWord.Length).ToArray();
            var firstWord = sentence.First();
            return firstWord.IsCapitalized
                ? sequence
                : preWord.Concat(sentence.Skip(1).Prepend(firstWord.Capitalize()));
        }

        //private static bool IsSentence(IList<ITranslation> translations)
        //    => translations.Any(t => t.From is Element) && IsEndOfSentence(translations.Last().From);

        private static bool IsSentence(IList<ITranslation> translations)
            => translations.Any() && IsStartOfSentence(translations.First().From) && IsEndOfSentence(translations.Last().From);

        private static bool IsStartOfSentence(Token token)
            => token is Element && char.IsUpper(token.Value.FirstOrDefault());

        private static bool IsEndOfSentence(Token token)
            => token is Terminator || token is Ellipsis;

        private static IEnumerable<ITranslation> PromoteInvisibleCapitalizations(
            ICollection<ITranslation> arrangedTranslations, IList<ITranslation> allTranslations)
            => arrangedTranslations
                .Select(t => PromoteInvisibleCapitalization(
                    t, GetPreviousTranslation(allTranslations, t)));

        private static ITranslation GetPreviousTranslation(IList<ITranslation> allTranslations,
            ITranslation translation)
        {
            var index = allTranslations.IndexOf(translation);
            return index > 0 ? allTranslations[index - 1] : null;
        }

        private static ITranslation PromoteInvisibleCapitalization(
            ITranslation translation, ITranslation previousTranslation)
            => ShouldPromoteInvisibleCapitalization(translation, previousTranslation)
                ? translation.Capitalize()
                : translation;

        private static bool ShouldPromoteInvisibleCapitalization(
            ITranslation translation, ITranslation previousTranslation)
            => !translation.IsCapitalized
               && previousTranslation != null
               && IsInvisibleCapitalized(previousTranslation);

        private static bool IsInvisibleCapitalized(ITranslation previousWord)
            => previousWord.IsCapitalized && string.IsNullOrEmpty(previousWord.Output);

    }
}