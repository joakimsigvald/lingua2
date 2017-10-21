using System.Collections.Generic;

namespace Lingua.Grammar
{
    public class ScoreTreeNode
    {
        public ScoreTreeNode(ushort code, ushort[] path, sbyte score, List<ScoreTreeNode> children)
        {
            Code = code;
            Path = path;
            Score = score;
            Children = children;
        }

        public readonly List<ScoreTreeNode> Children;
        public readonly ushort Code;
        public readonly ushort[] Path;
        public sbyte Score { get; set; }

        public override string ToString()
            => string.Join(",", Path);
    }
}