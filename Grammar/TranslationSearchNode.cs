namespace Lingua.Grammar
{
    using Core;

    internal class TranslationSearchNode
    {
        public static TranslationSearchNode[] NoChildren = new TranslationSearchNode[0];

        public ITranslation? Translation { get; internal set; }
        public TranslationSearchNode[] Children { get; set; } = NoChildren;
        public byte WordCount { get; internal set; }
        public int Score { get; set; }
        public int BestScore { get; set; } = int.MinValue;
        public byte Index { get; internal set; }
        public ushort[] ReversedCode { get; internal set; } = new ushort[0];
    }
}