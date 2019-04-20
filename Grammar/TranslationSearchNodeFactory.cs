using Lingua.Core;

namespace Lingua.Grammar
{
    internal class TranslationSearchNodeFactory
    {
        private readonly CodeCondenser _codeReducer = new CodeCondenser();

        public TranslationSearchNode Create(TranslationSearchNode parent, IGrammaton grammaton)
            => new TranslationSearchNode
            {
                Grammaton = grammaton,
                WordCount = grammaton.WordCount,
                Index = (byte)(parent.Index + parent.WordCount),
                ReversedCode = ExtractReversedCode(parent, grammaton),
                Score = parent.Score
            };

        private ushort[] ExtractReversedCode(TranslationSearchNode parent, IGrammaton grammaton)
            => _codeReducer.Condense(parent.ReversedCode, grammaton);
    }
}