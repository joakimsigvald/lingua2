using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    public class TranslationTreeNode
    {
        private IEnumerable<TranslationTreeNode> _children;
        private readonly Func<IEnumerable<TranslationTreeNode>> _getChildren;

        public TranslationTreeNode(Translation translation, ushort code, Func<IEnumerable<TranslationTreeNode>> getChildren)
        {
            Translation = translation;
            Code = code;
            _getChildren = getChildren;
        }

        public readonly Translation Translation;
        public readonly ushort Code;
        public IEnumerable<TranslationTreeNode> Children
            => _children ?? (_children = _getChildren().ToList());
    }
}
