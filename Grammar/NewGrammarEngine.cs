using System.Collections.Generic;

namespace Lingua.Grammar
{
    using Core;
    using Lingua.Core.Extensions;
    using System.Linq;

    public class NewGrammarEngine : IGrammar
    {
        private static readonly Reason _emptyReason = new Reason(new ushort[0], new IEvaluation[0]);

        private readonly IEvaluator _evaluator;
        public NewGrammarEngine(IEvaluator evaluator) => _evaluator = evaluator;

        public (ITranslation[] Translations, IReason Reason) Reduce(IList<ITranslation[]> possibilities)
        {
            var translations = GetTranslations(possibilities.ToArray()).ToArray();
            return (translations, _emptyReason);
        }

        private IList<ITranslation> GetTranslations(ITranslation[][] possibilities)
        {
            var translations = new List<ITranslation>();
            if (!possibilities.Any())
                return translations;
            TranslationSearchNode? tree = CreateTree(possibilities);
            do
            {
                translations.Add(tree!.Translation!);
                tree = SelectBestBranch(tree, possibilities);
            }
            while (tree != null);
            return translations;
        }

        private TranslationSearchNode CreateTree(ITranslation[][] possibilities)
        {
            var root = new TranslationSearchNode();
            var bestChild = new TranslationSearchNode();
            foreach (var cand in possibilities[0])
            {
                var nextChild = CreateChild(root, cand, possibilities, _evaluator.Horizon);
                if (nextChild.BestScore > bestChild.BestScore)
                    bestChild = nextChild;
            }
            return bestChild;
        }

        private TranslationSearchNode CreateChild(TranslationSearchNode parent, ITranslation candidate, ITranslation[][] possibilities, byte horizon)
        {
            var node = new TranslationSearchNode(parent, candidate, _evaluator.Horizon);
            node.Score += _evaluator.EvaluateReversed(node.ReversedCode);
            node.BestScore = node.Score;
            Populate(node, possibilities, (byte)(horizon - 1));
            return node;
        }

        private void Populate(TranslationSearchNode node, ITranslation[][] possibilities, byte horizon)
        {
            var nextIndex = node.Index + node.WordCount;
            if (possibilities.Length <= nextIndex || horizon == 0)
                return;
            node.Children = possibilities[nextIndex]
                .Select(cand => CreateChild(node, cand, possibilities, horizon))
                .ToArray();
            node.BestScore = node.Children.Max(child => child.BestScore);
        }

        private TranslationSearchNode? SelectBestBranch(TranslationSearchNode root, ITranslation[][] possibilities)
        {
            if (!root.Children.Any())
                return null;
            var bestChild = new TranslationSearchNode();
            foreach (var child in root.Children)
            {
                ExpandChild(child, possibilities, _evaluator.Horizon);
                if (child.BestScore > bestChild.BestScore)
                    bestChild = child;
            }
            return bestChild;
        }

        private void ExpandChild(TranslationSearchNode node, ITranslation[][] possibilities, byte horizon)
        {
            if (node.Children.Any())
                Expand(node, possibilities, (byte)(horizon - 1));
            else
                Populate(node, possibilities, horizon);
        }

        private void Expand(TranslationSearchNode node, ITranslation[][] possibilities, byte horizon)
        {
            node.Children.ForEach(child => ExpandChild(child, possibilities, horizon));
            node.BestScore = node.Children.Max(child => child.BestScore);
        }
    }
}