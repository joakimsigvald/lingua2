using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public class Evaluator : IEvaluator
    {
        public ScoreTreeNode ScoringTree;

        private static readonly Lazy<ScoreTreeNode> LoadedScoringTree =
            new Lazy<ScoreTreeNode>(() => BuildScoringTree(Repository.LoadScoredPatterns()));

        public Evaluator(IDictionary<string, sbyte>? patterns = null)
        {
            ScoringTree = BuildScoringTree(patterns ?? new Dictionary<string, sbyte>());
        }

        public byte Horizon => 6;

        public void Load()
        {
            ScoringTree = LoadedScoringTree.Value;
        }

        public IEvaluation Evaluate(ushort[] code, int commonLength = 0)
        {
            var scorings = GetMatchingScoreNodes(code, commonLength)
                .GroupBy(n => n)
                .Select(n => new Scoring(n.Key.Path, (byte)n.Count(), n.Key.Score))
                .ToArray();
            return new Evaluation(scorings);
        }

        private IEnumerable<ScoreTreeNode> GetMatchingScoreNodes(ushort[] sequence, int commonLength)
            => new EvaluationProcess(sequence, commonLength).GetMatchingScoreNodes(ScoringTree);

        private static ScoreTreeNode BuildScoringTree(IDictionary<string, sbyte> patterns)
        {
            var path = new ushort[0];
            return new ScoreTreeNode(0, path, 0, BuildScoringNodes(EncodePatterns(patterns), path).ToList());
        }

        private static IEnumerable<(ushort[], sbyte)> EncodePatterns(IDictionary<string, sbyte> patterns)
            => patterns.Select(kvp => (Encoder.Encode(kvp.Key).ToArray(), kvp.Value));

        private static IEnumerable<ScoreTreeNode> BuildScoringNodes(IEnumerable<(ushort[], sbyte)> codedPatterns, ushort[] path, int index = 0)
            => codedPatterns
                .Where(t => t.Item1.Length > index)
                .GroupBy(item => item.Item1[index])
                .Select(g => BuildScoringNode(g, path.Append(g.Key).ToArray(), index + 1));

        private static ScoreTreeNode BuildScoringNode(IGrouping<ushort, (ushort[], sbyte)> g, 
            ushort[] path, 
            int index)
            => new ScoreTreeNode(
                    g.Key,
                    path,
                    GetNodeScore(g, index),
                    BuildScoringNodes(g, path, index).ToList());

        private static sbyte GetNodeScore(IEnumerable<(ushort[], sbyte)> codeScores, int index)
            => codeScores.SingleOrDefault(v => v.Item1.Length == index).Item2;

        public int EvaluateReversed(ushort[] invertedCode)
        {
            return 0;
        }
    }
}