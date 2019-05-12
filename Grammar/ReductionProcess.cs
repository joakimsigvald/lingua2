using System.Collections.Generic;

namespace Lingua.Grammar
{
    using Core;
    using Lingua.Core.Extensions;
    using Lingua.Core.Tokens;
    using System.Linq;

    internal class ReductionProcess
    {
        private readonly IEvaluator _evaluator;
        private readonly IDecomposition _decomposition;
        private readonly TranslationSearchNodeFactory _nodeFactory;

        public ReductionProcess(IEvaluator evaluator, IDecomposition decomposition)
        {
            _evaluator = evaluator;
            _decomposition = decomposition;
            _nodeFactory = new TranslationSearchNodeFactory(evaluator);
        }

        public ReductionResult Reduce()
        {
            var grammatons = new List<IGrammaton>();
            TranslationSearchNode best;
            TranslationSearchNode? next = CreateTree();
            do
            {
                grammatons.Add((best = next!).Grammaton!);
                next = SelectBestBranch(best);
            }
            while (next != null);
            return new ReductionResult(grammatons.ToArray(), best.ReversedCode, best.Score);
        }

        private TranslationSearchNode CreateTree()
        {
            var root = new TranslationSearchNode
            {
                ReversedCode = new[] { Start.Code }
            };
            var bestChild = new TranslationSearchNode();
            foreach (var cand in _decomposition[0].GrammatonCandidates)
            {
                var nextChild = CreateChild(root, cand, _evaluator.Horizon);
                if (nextChild.BestScore > bestChild.BestScore)
                    bestChild = nextChild;
            }
            return bestChild;
        }

        private TranslationSearchNode CreateChild(TranslationSearchNode parent, IGrammatonCandidate candidate, byte horizon)
        {
            var node = _nodeFactory.Create(parent, candidate);
            ExpandChild(node, (byte)(horizon - 1));
            return node;
        }

        private TranslationSearchNode? SelectBestBranch(TranslationSearchNode root)
        {
            if (!root.Children.Any())
                return null;
            var bestChild = new TranslationSearchNode();
            foreach (var child in root.Children)
            {
                ExpandChild(child, _evaluator.Horizon);
                if (child.BestScore > bestChild.BestScore)
                    bestChild = child;
            }
            return bestChild;
        }

        private void ExpandChild(TranslationSearchNode node, byte horizon)
        {
            if (node.Children.Any())
                ExpandChildren(node, (byte)(horizon - 1));
            else
                CreateChildren(node, horizon);
            node.BestScore = node.Children.Any() 
                ? node.Children.Max(child => child.BestScore)
                : node.Score;
        }

        private void ExpandChildren(TranslationSearchNode node, byte horizon)
        {
            node.Children.ForEach(child => ExpandChild(child, horizon));
        }

        private void CreateChildren(TranslationSearchNode node, byte horizon)
        {
            var nextIndex = node.Index + node.WordCount;
            if (_decomposition.Length <= nextIndex || horizon == 0)
                return;
            node.Children = _decomposition[nextIndex].GrammatonCandidates
                .Select(cand => CreateChild(node, cand, horizon))
                .ToArray();
        }
    }
}