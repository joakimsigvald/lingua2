using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;

namespace Lingua.Learning
{
    using Core;
    using Core.Extensions;
    using Grammar;

    public class TrainableEvaluator : Evaluator
    {
        private ushort _nextAggregateCode = Aggregate.MinCode;

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
            Repository.StoreScoredPatterns(ScoringTree.PatternLines);
            Repository.StoreRearrangements(Arrangers);
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

        public sbyte GetScore(ushort[] code)
            => GetScoreNode(ScoringTree, code, 0)?.Score ?? 0;

        private static ScoreTreeNode GetScoreNode(ScoreTreeNode node, ushort[] code, int index)
        {
            if (code.Length == index)
                return node;
            var next = code[index];
            var child = node.Children.FirstOrDefault(c => c.Code == next);
            return child == null ? null : GetScoreNode(child, code, index + 1);
        }

        public void AggregatePatterns()
        {
            AddAggregates(GetSubPatterns());
        }

        private IEnumerable<ushort[]> GetSubPatterns()
            => ScoringTree.Patterns
                .SelectMany(GetSubPatterns)
                .GroupBy(p => p, new CodeComparer())
                .Where(g => g.Count() > 2)
                .Select(g => g.Key);

        private void AddAggregates(IEnumerable<ushort[]> subPatterns)
        {
            foreach (var subPattern in subPatterns)
            {
                if (_nextAggregateCode == 0)
                    break;
                var aggregate = new Aggregate(_nextAggregateCode++, subPattern);
                AddAggregate(aggregate);
            }
        }

        private void AddAggregate(Aggregate aggregate)
        {
            Aggregates.Add(aggregate);
            ScoringTree.Replace(aggregate.Pattern, aggregate.Code);
        }

        private static IEnumerable<ushort[]> GetSubPatterns(ushort[] pattern)
            => Enumerable.Range(1, pattern.Length - 2)
                .SelectMany(o => GetStartingSubPatterns(pattern.Skip(o).ToArray()))
                .Concat(GetStartingSubPatterns(pattern));

        private static IEnumerable<ushort[]> GetStartingSubPatterns(ushort[] pattern)
            => Enumerable.Range(2, pattern.Length - 1).Select(l => pattern.Take(l).ToArray());
    }

    public class CodeComparer : IEqualityComparer<ushort[]>
    {
        public bool Equals(ushort[] x, ushort[] y)
            => x == null 
            ? y == null 
            : y != null && x.SequenceEqual(y);

        public int GetHashCode(ushort[] code)
            => code.Sum(c => c);
    }
}