using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Grammar;

    public interface ITrainableEvaluator {
        int ComputeScoreDeficit(TestCaseResult failedCase);
        void Do(ScoredPattern scoredPattern);
        void Undo(ScoredPattern scoredPattern);
        void Add(Arranger arranger);
        void Remove(Arranger arranger);
        sbyte GetScore(ushort[] code);
    }

    public class TrainableEvaluator : Evaluator, ITrainableEvaluator 
    {
        private Rearranger _arranger;

        public TrainableEvaluator(Rearranger arranger) => _arranger = arranger;

        public void UpdateScore(ushort[] code, sbyte addScore)
        {
            if (addScore == 0)
                return;
            UpdateScore(ScoringTree, (code, addScore), 0);
        }

        public void Do(ScoredPattern scoredPattern)
        {
            UpdateScore(scoredPattern.Code, scoredPattern.Score);
        }

        public void Undo(ScoredPattern scoredPattern)
        {
            UpdateScore(scoredPattern.Code, (sbyte) -scoredPattern.Score);
        }

        public void Add(Arranger arranger)
        {
            _arranger.Add(arranger);
        }

        public void Remove(Arranger arranger)
        {
            _arranger.Remove(arranger);
        }

        public int ComputeScoreDeficit(TestCaseResult failedCase)
        {
            var expected = Encoder.Encode(failedCase.ExpectedTranslations).ToArray();
            var actual = Encoder.Encode(failedCase.Translations).ToArray();
            var expectedScore = Evaluate(expected).Score;
            var actualScore = Evaluate(actual).Score;
            return actualScore - expectedScore;
        }

        public sbyte GetScore(ushort[] code)
            => GetScoreNode(ScoringTree, code, 0)?.Score ?? 0;

        public void SavePatterns()
        {
            Repository.StoreScoredPatterns(ScoringTree.PatternLines);
            Repository.StoreRearrangements(_arranger.Arrangers);
        }

        private static void UpdateScore(ScoreTreeNode node, (ushort[], sbyte) codedPattern, int index)
        {
            if (codedPattern.Item1.Length == index)
                node.Score += codedPattern.Item2;
            else
            {
                var next = codedPattern.Item1[index];
                var child = node.Children.FirstOrDefault(c => c.Code == next);
                if (child == null)
                {
                    child = new ScoreTreeNode(next, node.Path.Append(next).ToArray(), 0, new List<ScoreTreeNode>());
                    node.AddChild(child);
                }
                UpdateScore(child, codedPattern, index + 1);
                if (child.Score == 0 && !child.Children.Any())
                    node.RemoveChild(child);
            }
        }

        private static ScoreTreeNode? GetScoreNode(ScoreTreeNode node, ushort[] code, int index)
        {
            if (code.Length == index)
                return node;
            var next = code[index];
            var child = node.Children.FirstOrDefault(c => c.Code == next);
            return child == null ? null : GetScoreNode(child, code, index + 1);
        }
    }
}