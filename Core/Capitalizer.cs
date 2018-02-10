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
            return SentenceCapitalizer.Capitalize(recapitalized);
        }

        private class Recapitalizer
        {
            private readonly List<ITranslation> _uncheckedTranslationsOriginalOrder;

            public Recapitalizer(IEnumerable<ITranslation> allTranslations)
                => _uncheckedTranslationsOriginalOrder = allTranslations.ToList();

            public IEnumerable<ITranslation> Recapitalize(IEnumerable<ITranslation> arrangedTranslations)
                => arrangedTranslations
                    .Where(t => !string.IsNullOrEmpty(t.Output))
                    .Select(Recapitalize);

            private ITranslation Recapitalize(ITranslation translation)
            {
                var originalIndex = _uncheckedTranslationsOriginalOrder.IndexOf(translation);
                if (originalIndex >= 0)
                    _uncheckedTranslationsOriginalOrder.RemoveAt(originalIndex);
                return Recapitalize(originalIndex, translation);
            }

            private ITranslation Recapitalize(int originalIndex, ITranslation translation)
                => !translation.IsCapitalized
                    ? PromoteCapitalization(translation, originalIndex)
                    : originalIndex < 0
                        ? translation.Decapitalize()
                        : translation;

            private ITranslation PromoteCapitalization(ITranslation tran,
                int index)
            {
                for (var i = 0; i < index; i++)
                {
                    var other = _uncheckedTranslationsOriginalOrder[i];
                    _uncheckedTranslationsOriginalOrder.RemoveAt(i);
                    if (other.IsCapitalized)
                        return tran.Capitalize();
                }
                return tran;
            }
        }

        private static class SentenceCapitalizer
        {
            public static IEnumerable<ITranslation> Capitalize(IEnumerable<ITranslation> translations)
                => SeparateSentences(translations)
                .Where(s => s.Any())
                .SelectMany(HandleSequence);

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

            private static IEnumerable<ITranslation> HandleSequence(IList<ITranslation> sequence) 
                => IsProperSentence(sequence) 
                ? CapitalizeStartOfSentence(sequence) 
                : sequence;

            private static IEnumerable<ITranslation> CapitalizeStartOfSentence(IList<ITranslation> sequence)
            {
                var leadingSymbols = sequence.TakeWhile(t => !(t.From is Element)).ToArray();
                var sentence = sequence.Skip(leadingSymbols.Length).ToArray();
                var firstWord = sentence.First();
                return firstWord.IsCapitalized
                    ? sequence
                    : leadingSymbols.Concat(sentence.Skip(1).Prepend(firstWord.Capitalize()));
            }

            private static bool IsProperSentence(IList<ITranslation> translations)
                => translations.First().From.Value.IsCapitalized() && IsEndOfSentence(translations.Last().From);

            private static bool IsEndOfSentence(Token token)
                => token is Terminator || token is Ellipsis;
        }
    }
}