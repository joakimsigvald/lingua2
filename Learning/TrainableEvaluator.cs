using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Extensions;

namespace Lingua.Learning
{
    using Core;
    using Grammar;

    public class TrainableEvaluator : Evaluator
    {
        public void UpdateScore(string pattern, sbyte addScore)
        {
            if (addScore == 0)
                return;
            UpdateScore(ScoringTree, CreateCodeScore(pattern, addScore), 0);
        }

        public void Do(ScoredPattern scoredPattern)
        {
            UpdateScore(scoredPattern.Pattern, scoredPattern.Score);
        }

        public void Undo(ScoredPattern scoredPattern)
        {
            UpdateScore(scoredPattern.Pattern, (sbyte)-scoredPattern.Score);
        }

        public void Add(Arranger arranger)
        {
            if (arranger != null && !Arrangers.Contains(arranger))
                Arrangers.Add(arranger);
        }

        public void Remove(Arranger arranger)
        {
            Arrangers.Remove(arranger);
        }

        public int ComputeScoreDeficit(TestCaseResult failedCase)
        {
            var expected = Encoder.Encode(failedCase.ExpectedTranslations).ToArray();
            var actual = Encoder.Encode(failedCase.Translations).ToArray();
            var expectedScore = Evaluate(expected).Score;
            var actualScore = Evaluate(actual).Score;
            return actualScore - expectedScore;
        }

        public void SavePatterns()
        {
            Repository.StoreScoredPatterns(ScoringTree.ToDictionary());
            Repository.StoreRearrangements(Arrangers);
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