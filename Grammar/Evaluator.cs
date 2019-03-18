using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;

namespace Lingua.Grammar
{
    using Core;

    public class Evaluator : IEvaluator, IArranger
    {
        public ScoreTreeNode ScoringTree;
        public IList<Arranger> Arrangers = new List<Arranger>();
        public List<Aggregate> Aggregates = new List<Aggregate>();

        private static readonly Lazy<ScoreTreeNode> LoadedScoringTree =
            new Lazy<ScoreTreeNode>(() => BuildScoringTree(Repository.LoadScoredPatterns()));

        private static readonly Lazy<IList<Arranger>> LoadedArrangers =
            new Lazy<IList<Arranger>>(() => BuildArrangers(Repository.LoadArrangements()));

        public Evaluator(IDictionary<string, sbyte>? patterns = null)
        {
            ScoringTree = BuildScoringTree(patterns ?? new Dictionary<string, sbyte>());
        }

        public byte Horizon => 6;

        public void Load()
        {
            ScoringTree = LoadedScoringTree.Value;
            Arrangers = LoadedArrangers.Value;
        }

        public IEvaluation Evaluate(ushort[] code, int commonLength = 0)
        {
            var scorings = GetMatchingScoreNodes(code, commonLength)
                .GroupBy(n => n)
                .Select(n => new Scoring(n.Key.Path, (byte)n.Count(), n.Key.Score))
                .ToArray();
            return new Evaluation(scorings);
        }

        public ITranslation[] Arrange(IEnumerable<ITranslation> translations)
            => DoArrange(translations.ToArray()).SelectMany(s => s).ToArray();

        private IEnumerable<IEnumerable<ITranslation>> DoArrange(ICollection<ITranslation> translations)
        {
            for (var i = 0; i < translations.Count;)
            {
                var remaining = translations.Skip(i).ToArray();
                (var arrangedSegment, var length) = ArrangeSegment(remaining);
                i += Math.Max(1, length);
                yield return arrangedSegment ?? remaining.Take(1);
            }
        }

        private (ITranslation[] arrangement, int length) ArrangeSegment(IList<ITranslation> remainingTranslations)
            => Arrangers
                .Select(arranger => Arrange(arranger, remainingTranslations))
                .FirstOrDefault(result => result.arrangement != null);

        private static (ITranslation[] arrangement, int length) Arrange(Arranger arr, IEnumerable<ITranslation> remainingTranslations)
        {
            var segment = remainingTranslations
                .Take(arr.Length)
                .ToArray();
            return arr.Arrange(segment);
        }

        private static IList<Arranger> BuildArrangers(IEnumerable<Arrangement> arrangements)
            => arrangements.Select(arr => new Arranger(arr)).ToList();

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

        public int EvaluateInverted(ushort[] invertedCode)
        {
            throw new NotImplementedException();
        }
    }
}