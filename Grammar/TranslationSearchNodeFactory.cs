using Lingua.Core;

namespace Lingua.Grammar
{
    internal class TranslationSearchNodeFactory
    {
        private readonly CodeCondenser _codeReducer = new CodeCondenser();

        public TranslationSearchNode Create(TranslationSearchNode parent, ITranslation translation)
            => new TranslationSearchNode
            {
                Translation = translation,
                WordCount = translation.WordCount,
                Index = (byte)(parent.Index + parent.WordCount),
                ReversedCode = ExtractReversedCode(parent, translation),
                Score = parent.Score
            };

        private ushort[] ExtractReversedCode(TranslationSearchNode parent, ITranslation translation)
            => _codeReducer.Condense(parent.ReversedCode, translation);
    }
}