﻿using System.Collections.Generic;
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
            var recapitalized = PromoteInvisibleCapitalizations(arrangedTranslations, allTranslations);
            return CapitalizeStartOfSentences(recapitalized);
        }

        private static IEnumerable<ITranslation> PromoteInvisibleCapitalizations(
            IEnumerable<ITranslation> arrangedTranslations,
            IEnumerable<ITranslation> allTranslations)
        {
            var remaining = allTranslations.ToList();
            foreach (var tran in arrangedTranslations)
            {
                var index = remaining.IndexOf(tran);
                if (string.IsNullOrEmpty(tran.Output))
                    continue;
                remaining.Remove(tran);
                if (index < 0 && tran.IsCapitalized)
                    yield return tran.Decapitalize();
                else if (!tran.IsCapitalized)
                    yield return PromoteInvisibleCapitalization(remaining, tran, index);
                else yield return tran;
            }
        }

        private static ITranslation PromoteInvisibleCapitalization(IList<ITranslation> remaining, ITranslation tran, int index)
        {
            for (var i = index - 1; i >= 0; i--)
            {
                var other = remaining[i];
                remaining.RemoveAt(i);
                if (other.IsCapitalized)
                    return tran.Capitalize();
            }
            return tran;
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
            => translations.Any() && IsStartOfSentence(translations.First().From) && IsEndOfSentence(translations.Last().From);

        private static bool IsStartOfSentence(Token token)
            => token is Element && char.IsUpper(token.Value.FirstOrDefault());

        private static bool IsEndOfSentence(Token token)
            => token is Terminator || token is Ellipsis;
    }
}