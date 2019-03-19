namespace Lingua.Grammar
{
    using Core;
    using System.Linq;

    public class Node
    {
        public static Node[] NoChildren = new Node[0];

        public Node()
        {
        }

        public Node(Node parent, ITranslation translation, byte horizon)
        {
            Translation = translation;
            WordCount = translation.WordCount;
            Index = (byte)(parent.Index + parent.WordCount);
            ReversedCode = parent.ReversedCode.Prepend(translation.Code).Take(horizon).ToArray();
            Score = parent.Score;
        }

        public ITranslation? Translation { get; }
        public Node[] Children { get; set; } = NoChildren;
        public byte WordCount { get; }
        public int Score { get; set; }
        public int BestScore { get; set; } = int.MinValue;
        public byte Index { get; }
        public ushort[] ReversedCode { get; } = new ushort[0];
    }
}