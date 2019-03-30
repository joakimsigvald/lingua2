using System.Collections.Generic;

namespace Lingua.Grammar
{
    using Core;
    using Lingua.Core.Extensions;
    using System.Linq;

    public class GrammarEngine : IGrammar
    {
        private readonly IEvaluator _evaluator;
        public GrammarEngine(IEvaluator evaluator) => _evaluator = evaluator;

        public ITranslation[] Reduce(IList<ITranslation[]> possibilities)
            => possibilities.Any()
            ? new GrammarProcess(_evaluator, possibilities.ToArray()).Reduce()
            : new ITranslation[0];
    }

    internal class GrammarProcess
    {
        private readonly IEvaluator _evaluator;
        private readonly ITranslation[][] _possibilities;
        private readonly TranslationSearchNodeFactory _nodeFactory;

        public GrammarProcess(IEvaluator evaluator, ITranslation[][] possibilities)
        {
            _evaluator = evaluator;
            _possibilities = possibilities;
            _nodeFactory = new TranslationSearchNodeFactory();
        }

        public ITranslation[] Reduce()
        {
            var translations = new List<ITranslation>();
            TranslationSearchNode? tree = CreateTree();
            do
            {
                translations.Add(tree!.Translation!);
                tree = SelectBestBranch(tree);
            }
            while (tree != null);
            return translations.ToArray();
        }

        private TranslationSearchNode CreateTree()
        {
            var root = new TranslationSearchNode();
            var bestChild = new TranslationSearchNode();
            foreach (var cand in _possibilities[0])
            {
                var nextChild = CreateChild(root, cand, _evaluator.Horizon);
                if (nextChild.BestScore > bestChild.BestScore)
                    bestChild = nextChild;
            }
            return bestChild;
        }

        private TranslationSearchNode CreateChild(TranslationSearchNode parent, ITranslation candidate, byte horizon)
        {
            var node = _nodeFactory.Create(parent, candidate, _evaluator.Horizon);
            node.Score += _evaluator.EvaluateReversed(node.ReversedCode);
            node.BestScore = node.Score;
            Populate(node, (byte)(horizon - 1));
            return node;
        }

        private void Populate(TranslationSearchNode node, byte horizon)
        {
            var nextIndex = node.Index + node.WordCount;
            if (_possibilities.Length <= nextIndex || horizon == 0)
                return;
            node.Children = _possibilities[nextIndex]
                .Select(cand => CreateChild(node, cand, horizon))
                .ToArray();
            node.BestScore = node.Children.Max(child => child.BestScore);
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
                Expand(node, (byte)(horizon - 1));
            else
                Populate(node, horizon);
        }

        private void Expand(TranslationSearchNode node, byte horizon)
        {
            node.Children.ForEach(child => ExpandChild(child, horizon));
            node.BestScore = node.Children.Max(child => child.BestScore);
        }
    }
}