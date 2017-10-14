using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    public class ScoreTreeNode : TreeNode<Tuple<ushort, ushort[], sbyte?>, ScoreTreeNode>
    {
        public ScoreTreeNode(ushort code, ushort[] path, sbyte? score, IEnumerable<ScoreTreeNode> children) 
            : base(new Tuple<ushort, ushort[], sbyte?>(code, path, score), children.ToList())
        {
        }

        public ushort Code => Value.Item1;
        public ushort[] Path => Value.Item2;
        public sbyte? Score => Value.Item3;

        public override string ToString()
            => string.Join(",", Path);
    }
}