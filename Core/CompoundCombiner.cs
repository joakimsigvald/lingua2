using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    using Extensions;
    using Tokens;

    public static class CompoundCombiner
    {
        public static IEnumerable<Translation[]> Combine(ICollection<Translation[]> alternatives)
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

        private static IEnumerable<Translation> CompleteCompound(Translation incompleteCompound,
            ICollection<Translation[]> future)
        {
            if (!future.Any() || !(future.First()[0].From is Word))
                return new Translation[0];
            var nextWords = future.First().Where(t => t.From.GetType() == incompleteCompound.From.GetType());
            return nextWords.Select(completion => CompleteCompound(incompleteCompound, completion));
        }

        private static Translation CompleteCompound(Translation incompleteCompound, Translation completion)
        {
            var completedFrom = ((Word)incompleteCompound.From).Clone();
            var fromCompletion = (Word)completion.From;
            completedFrom.Modifiers = fromCompletion.Modifiers;
            return new Translation
            {
                From = completedFrom,
                To = incompleteCompound.To + completion.To,
                IsIncompleteCompound = completion.IsIncompleteCompound,
                Continuation = completion.Continuation.Prepend((Word)completion.From).ToArray()
            };
        }
    }
}