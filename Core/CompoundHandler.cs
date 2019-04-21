using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    using Extensions;
    using Tokens;

    public static class CompoundHandler
    {
        public static IEnumerable<ITranslation[]> CompleteCompounds(IEnumerable<ITranslation[]> translationCandidates, Token[] tokens)
        {
            var filteredCandidates = FilterCompounds(translationCandidates, tokens).ToArray();
            return MergeCompounds(filteredCandidates);
        }

        private static IEnumerable<ITranslation[]> FilterCompounds(IEnumerable<ITranslation[]> alternatives,
            IReadOnlyList<Token> tokens)
            => alternatives.Select((candidates, ai) => FilterCompounds(candidates, tokens, ai + 1).ToArray());

        private static IEnumerable<ITranslation> FilterCompounds(IEnumerable<ITranslation> candidates,
            IReadOnlyList<Token> tokens,
            int nextIndex)
            => candidates.Where(t => t.Matches(tokens, nextIndex));

        private static IEnumerable<ITranslation[]> MergeCompounds(ICollection<ITranslation[]> alternatives)
        {
            return alternatives.Select((translations, i) =>
            {
                var incompleteCompounds = translations.Where(t => t.IsIncompleteCompound).ToArray();
                var words = translations.Except(incompleteCompounds);
                var compounds =
                    incompleteCompounds.SelectMany(
                        ic => CompleteCompound(ic, alternatives.Skip(i + ic.WordCount).ToArray()));
                return words.Concat(compounds).ToArray();
            });
        }

        private static IEnumerable<ITranslation> CompleteCompound(ITranslation incompleteCompound,
            ICollection<ITranslation[]> future)
        {
            if (!future.Any() || !(future.First()[0].From is Word))
                return new ITranslation[0];
            var nextWords = future.First().Where(t => t.From.GetType() == incompleteCompound.From.GetType());
            return nextWords.Select(completion => CompleteCompound(incompleteCompound, completion));
        }

        private static ITranslation CompleteCompound(ITranslation incompleteCompound, ITranslation completion)
        {
            var completedFrom = ((Word)incompleteCompound.From).Clone();
            var fromCompletion = (Word)completion.From;
            completedFrom.Modifiers = fromCompletion.Modifiers;
            return new Translation(completedFrom, incompleteCompound.To + completion.To.ToLower(), completion.Continuation.Prepend((Word)completion.From).ToArray())
            {
                IsIncompleteCompound = completion.IsIncompleteCompound,
                IsCapitalized = incompleteCompound.IsCapitalized
            };
        }
    }
}