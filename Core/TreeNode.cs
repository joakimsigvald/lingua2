using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    public class TreeNode<TValue>
    {
        public readonly TValue Value;

        private IList<TreeNode<TValue>> _children;
        private readonly Func<IEnumerable<TreeNode<TValue>>> _getChildren;

        public TreeNode(TValue value, Func<IEnumerable<TreeNode<TValue>>> getChildren)
        {
            Value = value;
            _getChildren = getChildren;
        }

        public IList<TreeNode<TValue>> Children
            => _children ?? (_children = _getChildren().ToList());
    }
}
