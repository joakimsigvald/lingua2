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

        private IEnumerable<ITranslation> GetTranslations(ITranslation[][] possibilities)
        {
            if (!possibilities.Any())
                yield break;
            Node? tree = CreateTree(possibilities);
            do
            {
                yield return tree.Translation!;
                tree = SelectBestBranch(tree, possibilities);
            }
            while (tree != null);
        }

        private Node CreateTree(ITranslation[][] possibilities)
        {
            var root = new Node();
            var bestChild = new Node();
            foreach (var cand in possibilities[0])
            {
                var nextChild = CreateChild(root, cand, possibilities, _evaluator.Horizon);
                if (nextChild.BestScore > bestChild.BestScore)
                    bestChild = nextChild;
            }
            return bestChild;
        }

        private Node CreateChild(Node parent, ITranslation candidate, ITranslation[][] possibilities, byte horizon)
        {
            var node = new Node(parent, candidate, _evaluator.Horizon);
            node.Score += _evaluator.EvaluateInverted(node.InvertedCode);
            node.BestScore = node.Score;
            Populate(node, possibilities, (byte)(horizon - 1));
            return node;
        }

        private void Populate(Node node, ITranslation[][] possibilities, byte horizon)
        {
            if (horizon == 0)
                return;
            var nextIndex = node.Index + node.WordCount;
            if (possibilities.Length <= nextIndex)
                return;
            node.Children = possibilities[nextIndex]
                .Select(cand => CreateChild(node, cand, possibilities, horizon))
                .ToArray();
            node.BestScore = node.Children.Max(child => child.BestScore);
        }

        private Node? SelectBestBranch(Node root, ITranslation[][] possibilities)
        {
            if (!root.Children.Any())
                return null;
            var bestChild = new Node();
            foreach (var child in root.Children)
            {
                ExpandChild(child, possibilities, _evaluator.Horizon);
                if (child.BestScore > bestChild.BestScore)
                    bestChild = child;
            }
            return bestChild;
        }

        private void ExpandChild(Node node, ITranslation[][] possibilities, byte horizon)
        {
            if (node.Children.Any())
                Expand(node, possibilities, (byte)(horizon - 1));
            else
                Populate(node, possibilities, horizon);
        }

        private void Expand(Node node, ITranslation[][] possibilities, byte horizon)
        {
            node.Children.ForEach(child => ExpandChild(child, possibilities, horizon));
            node.BestScore = node.Children.Max(child => child.BestScore);
        }
    }

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
            InvertedCode = parent.InvertedCode.Prepend(translation.Code).Take(horizon).ToArray();
            Score = parent.Score;
        }

        public ITranslation? Translation { get; }
        public Node[] Children { get; set; } = NoChildren;
        public int WordCount { get; }
        public int Score { get; set; }
        public int BestScore { get; set; } = int.MinValue;
        public byte Index { get; }
        public ushort[] InvertedCode { get; } = new ushort[0];
    }
}