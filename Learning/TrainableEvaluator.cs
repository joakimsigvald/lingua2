using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    using Core;
    using Grammar;

    public class TrainableEvaluator : Evaluator
    {
        public TrainableEvaluator() : base(new Dictionary<string, sbyte>())
        {
        }

        public void UpdateScore(string pattern, sbyte addScore)
        {
            if (addScore == 0)
                return;
            UpdateScore(ScoringTree, CreateCodeScore(pattern, addScore), 0);
        }

        private static void UpdateScore(ScoreTreeNode node, (string, ushort[], sbyte) codedPattern, int index)
        {
            if (codedPattern.Item2.Length == index)
                node.Score += codedPattern.Item3;
            else
            {
                var next = codedPattern.Item2[index];
                var child = node.Children.FirstOrDefault(c => c.Code == next);
                if (child == null)
                {
                    child = new ScoreTreeNode(next, node.Path.Append(next).ToArray(), 0, new List<ScoreTreeNode>());
                    node.Children.Add(child);
                }
                UpdateScore(child, codedPattern, index + 1);
                if (child.Score == 0 && !child.Children.Any())
                    node.Children.Remove(child);
            }
        }
    }
}