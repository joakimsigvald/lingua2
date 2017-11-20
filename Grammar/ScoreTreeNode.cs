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

        public int ScoredNodeCount => (Score == 0 ? 0 : 1) + Children.Sum(child => child.ScoredNodeCount); 

        public override string ToString()
            => $"{Encoder.Serialize(Path)}: {Score}";
    }
}