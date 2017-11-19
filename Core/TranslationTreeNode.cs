using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    using Tokens;

    public class TranslationTreeNode
    {
        private readonly Lazy<IList<TranslationTreeNode>> _lazyChildren;

        public TranslationTreeNode(Translation current, ICollection<Translation[]> futureAlternatives, bool completeCompounds = false)
        {
            Translation = current;
            Code = current == null ? Start.Code : Encoder.Encode(current.From);
            _lazyChildren = new Lazy<IList<TranslationTreeNode>>(() => Combine(futureAlternatives, completeCompounds));
        }

        public readonly Translation Translation;
        public readonly ushort Code;

        public IList<TranslationTreeNode> Children => _lazyChildren.Value;

        public IEnumerable<IList<TranslationTreeNode>> Expand(int depth)
            => ExpandChildren(depth).Select(seq => seq.Prepend(this).ToList());

        public IEnumerable<IList<TranslationTreeNode>> ExpandChildren(int depth)
            => depth > 0 && Children.Any()
                ? Children
                    .SelectMany(child => child.Expand(depth - 1))
                : new[] { new TranslationTreeNode[0] };

        private static IList<TranslationTreeNode> Combine(ICollection<Translation[]> alternatives, bool completeCompounds)
            => alternatives.Any()
                ? CombineRemaining(alternatives, completeCompounds)
                : new List<TranslationTreeNode>();

        private static IList<TranslationTreeNode> CombineRemaining(ICollection<Translation[]> alternatives
            , bool completeCompounds)
            => CreateSubtrees(completeCompounds ? CompleteFirst(alternatives) : alternatives.First()
                , alternatives
                , completeCompounds);

        private static IEnumerable<Translation> CompleteFirst(ICollection<Translation[]> alternatives)
        {
            var incompleteCompounds = alternatives.First().Where(t => t.IsIncompleteCompound).ToArray();
            var words = alternatives.First().Except(incompleteCompounds);
            var compounds =
                incompleteCompounds.SelectMany(ic => CompleteCompound(ic, alternatives.Skip(ic.WordCount).ToArray()));
            return words.Concat(compounds);
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

        private static IList<TranslationTreeNode> CreateSubtrees(IEnumerable<Translation> first
            , ICollection<Translation[]> alternatives
            , bool completeCompounds)
            => first
                .Select(t => CreateSubtree(t, alternatives, completeCompounds))
                .ToList();

        private static TranslationTreeNode CreateSubtree(Translation translation
            , IEnumerable<Translation[]> alternatives
            , bool completeCompounds)
            => new TranslationTreeNode(translation, alternatives.Skip(translation.WordCount).ToList(), completeCompounds);
    }
}