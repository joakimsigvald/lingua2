using System.Linq;

namespace Lingua.Learning
{
    using Grammar;
    using System;

    public class NewTrainableEvaluator : ITrainableEvaluator
    {
        private Rearranger _arranger;
        private readonly Evaluator _evaluator;

        public NewTrainableEvaluator(Rearranger arranger, Evaluator evaluator)
        {
            _arranger = arranger;
            _evaluator = evaluator;
        }

        public ReverseCodeScoreNode Patterns => _evaluator.Patterns;

        public void Do(ScoredPattern scoredPattern)
        {
            UpdateScore(scoredPattern.ReversedCode, scoredPattern.Score);
        }

        public void Undo(ScoredPattern scoredPattern)
        {
            UpdateScore(scoredPattern.ReversedCode, (sbyte) -scoredPattern.Score);
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
            var expectedScore = _evaluator.EvaluateReversed(failedCase.ExpectedReversedCode);
            var actualScore = _evaluator.EvaluateReversed(failedCase.ActuaReversedCode);
            return actualScore - expectedScore;
            return Math.Abs(actualScore - expectedScore);
        }

        public sbyte GetScore(ushort[] reversedCode)
            => GetScoreNode(_evaluator.Patterns, reversedCode, 0)?.Score ?? 0;

        public void SavePatterns()
        {
            Repository.StoreScoredPatterns(_evaluator.Patterns.PatternLines);
            Repository.StoreRearrangements(_arranger.Arrangers);
        }

        public void UpdateScore(ushort[] reversedCode, sbyte addScore)
        {
            if (addScore == 0)
                return;
            UpdateScore(_evaluator.Patterns, reversedCode, addScore, 0);
        }

        private static void UpdateScore(ReverseCodeScoreNode node, ushort[] reversedCode, sbyte score, int index)
        {
            if (reversedCode.Length == index)
                node.Score += score;
            else
            {
                var next = reversedCode[index++];
                var child = node.Previous.FirstOrDefault(c => c.Code == next);
                if (child == null)
                {
                    child = new ReverseCodeScoreNode(next);
                    child.Extend(reversedCode, score, index);
                    node.Previous.Add(child);
                    return;
                }
                UpdateScore(child, reversedCode, score, index);
                if (child.Score == 0 && !child.Previous.Any())
                    node.Previous.Remove(child);
            }
        }

        private static ReverseCodeScoreNode? GetScoreNode(ReverseCodeScoreNode node, ushort[] reversedCode, int index)
        {
            if (reversedCode.Length == index)
                return node;
            var next = reversedCode[index++];
            var child = node.Previous.FirstOrDefault(c => c.Code == next);
            return child == null ? null : GetScoreNode(child, reversedCode, index);
        }
    }
}