namespace Lingua.Grammar
{
    public class ScoreTreeNode
    {
        public ScoreTreeNode(ushort code, ushort[] path, sbyte? score, ScoreTreeNode[] children)
        {
            Code = code;
            Path = path;
            Score = score;
            Children = children;
        }

        public readonly ScoreTreeNode[] Children;
        public readonly ushort Code;
        public readonly ushort[] Path;
        public readonly sbyte? Score;

        public override string ToString()
            => string.Join(",", Path);
    }
}