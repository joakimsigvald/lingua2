namespace Lingua.Grammar
{
    using Core;
    using System.Linq;

    internal class TranslationSearchNode
    {
        public static TranslationSearchNode[] NoChildren = new TranslationSearchNode[0];

        public TranslationSearchNode()
        {
        }

        public TranslationSearchNode(TranslationSearchNode parent, ITranslation translation, byte horizon)
        {
            Translation = translation;
            WordCount = translation.WordCount;
            Index = (byte)(parent.Index + parent.WordCount);
            ReversedCode = parent.ReversedCode.Prepend(translation.Code).Take(horizon).ToArray();
            Score = parent.Score;
        }

        public ITranslation? Translation { get; }
        public TranslationSearchNode[] Children { get; set; } = NoChildren;
        public byte WordCount { get; }
        public int Score { get; set; }
        public int BestScore { get; set; } = int.MinValue;
        public byte Index { get; }
        public ushort[] ReversedCode { get; } = new ushort[0];
    }
}