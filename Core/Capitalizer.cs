using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Extensions;
using Lingua.Core.Tokens;

namespace Lingua.Core
{
    public class Capitalizer : ICapitalizer
    {
        public IEnumerable<ITranslation> Capitalize(IList<ITranslation> arrangedTranslations,
            IList<ITranslation> allTranslations)
        {
            var recapitalized = new Recapitalizer(allTranslations).Recapitalize(arrangedTranslations);
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

        private static bool IsSentence(IList<ITranslation> translations)
            => translations.Any() && IsStartOfSentence(translations.First().From) &&
               IsEndOfSentence(translations.Last().From);

        private static bool IsStartOfSentence(Token token)
            => token is Element && char.IsUpper(token.Value.FirstOrDefault());

        private static bool IsEndOfSentence(Token token)
            => token is Terminator || token is Ellipsis;

        private class Recapitalizer
        {
            private readonly List<ITranslation> _uncheckedTranslationsOriginalOrder;

            public Recapitalizer(IEnumerable<ITranslation> allTranslations)
                => _uncheckedTranslationsOriginalOrder = allTranslations.ToList();

            public IEnumerable<ITranslation> Recapitalize(IEnumerable<ITranslation> arrangedTranslations)
                => arrangedTranslations
                    .Select(Recapitalize)
                    .ExceptNull();

            private ITranslation Recapitalize(ITranslation translation)
            {
                var originalIndex = _uncheckedTranslationsOriginalOrder.IndexOf(translation);
                if (string.IsNullOrEmpty(translation.Output))
                    return null;
                _uncheckedTranslationsOriginalOrder.Remove(translation);
                return Recapitalize(originalIndex, translation);
            }

            private ITranslation Recapitalize(int originalIndex, ITranslation translation)
                => !translation.IsCapitalized
                    ? PromoteInvisibleCapitalization(translation, originalIndex)
                    : originalIndex < 0
                        ? translation.Decapitalize()
                        : translation;

            private ITranslation PromoteInvisibleCapitalization(ITranslation tran,
                int index)
            {
                for (var i = index - 1; i >= 0; i--)
                {
                    var other = _uncheckedTranslationsOriginalOrder[i];
                    _uncheckedTranslationsOriginalOrder.RemoveAt(i);
                    if (other.IsCapitalized)
                        return tran.Capitalize();
                }
                return tran;
            }
        }
    }
}