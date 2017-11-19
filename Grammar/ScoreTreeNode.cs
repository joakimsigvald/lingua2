using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

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
        public int LeafCount => Children.Any() ? Children.Sum(child => child.LeafCount) : 1;

        public override string ToString()
            => $"{Encoder.Serialize(Path)}: {Score}";
    }
}