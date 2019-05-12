using Lingua.Core;

namespace Lingua.Grammar
{
    internal class TranslationSearchNodeFactory
    {
        private readonly CodeCondenser _codeReducer = new CodeCondenser();
        private readonly IEvaluator _evaluator;

        public TranslationSearchNodeFactory(IEvaluator evaluator) => _evaluator = evaluator;

        public TranslationSearchNode Create(TranslationSearchNode parent, IGrammatonCandidate candidate) 
            => CreateChild(parent, (byte)(parent.Index + parent.WordCount), candidate, 0)!;

        private TranslationSearchNode? CreateChild(TranslationSearchNode parent, byte wordIndex, IGrammatonCandidate candidate, int sequenceIndex)
        {
            if (sequenceIndex == candidate.Length)
                return null;
            var next = candidate[sequenceIndex];
            var node = new TranslationSearchNode
            {
                Grammaton = next,
                WordCount = next.WordCount,
                Index = wordIndex,
                ReversedCode = ExtractReversedCode(parent, next)
            };
            node.Score = ComputeNodeScore(parent, node);
            var child = CreateChild(node, wordIndex, candidate, sequenceIndex + 1);
            if (child != null)
                node.Children = new[] { child };
            return node;
        }

        private int ComputeNodeScore(TranslationSearchNode parent, TranslationSearchNode node)
            => parent.Score + _evaluator.ScorePatternsEndingWith(node.ReversedCode);

        private ushort[] ExtractReversedCode(TranslationSearchNode parent, IGrammaton grammaton)
            => _codeReducer.Condense(parent.ReversedCode, grammaton);
    }
}