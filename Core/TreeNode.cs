using System.Collections.Generic;

namespace Lingua.Core
{
    public interface ITreeNode<out TChild>
    {
        IEnumerable<TChild> Children { get; }
    }

    public class TreeNode<TValue, TChild> : ITreeNode<TChild>
    {
        public readonly TValue Value;

        protected IList<TChild> _children;

        protected TreeNode(TValue value, IList<TChild> children)
        {
            Value = value;
            _children = children;
        }

        public virtual IEnumerable<TChild> Children
            => _children;
    }
}