using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    using Extensions;
    using Tokens;

    public class TranslationTreeNode
    {
        private readonly Lazy<IList<TranslationTreeNode>> _lazyChildren;

        public TranslationTreeNode(Translation current, ICollection<Translation[]> futureAlternatives)
        {
            Translation = current;
            Code = current == null ? Start.Code : Encoder.Encode(current.From);
            _lazyChildren = new Lazy<IList<TranslationTreeNode>>(() => Combine(futureAlternatives));
        }

        public readonly Translation Translation;
        public readonly ushort Code;

        public IList<TranslationTreeNode> Children => _lazyChildren.Value;

        public IEnumerable<IList<TranslationTreeNode>> Expand(int depth)
            => ExpandChildren(depth).Select(seq => seq.Prepend(this).ToList());

        private IEnumerable<IList<TranslationTreeNode>> ExpandChildren(int depth)
            => depth > 0 && Children.Any()
                ? Children
                    .SelectMany(child => child.Expand(depth - 1))
                : new[] { new TranslationTreeNode[0] };

        private static IList<TranslationTreeNode> Combine(ICollection<Translation[]> alternatives)
            => alternatives.Any()
                ? CombineRemaining(alternatives)
                : new List<TranslationTreeNode>();

        private static IList<TranslationTreeNode> CombineRemaining(ICollection<Translation[]> alternatives)
            => CreateSubtrees(alternatives.First(), alternatives);

        private static IList<TranslationTreeNode> CreateSubtrees(IEnumerable<Translation> first
            , ICollection<Translation[]> alternatives)
            => first
                .Select(t => CreateSubtree(t, alternatives))
                .ToList();

        private static TranslationTreeNode CreateSubtree(Translation translation
            , IEnumerable<Translation[]> alternatives)
            => new TranslationTreeNode(translation, alternatives.Skip(translation.WordCount).ToList());
    }
}