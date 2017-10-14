using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    public class LazyTreeNode<TValue> : TreeNode<TValue, LazyTreeNode<TValue>>
    {
        private readonly Func<IEnumerable<LazyTreeNode<TValue>>> _getChildren;

        public LazyTreeNode(TValue value, Func<IEnumerable<LazyTreeNode<TValue>>> getChildren)
            : base(value, null)
        {
            _getChildren = getChildren;
        }

        public override IEnumerable<LazyTreeNode<TValue>> Children
            => _children ?? (_children = _getChildren().ToList());
    }
}
