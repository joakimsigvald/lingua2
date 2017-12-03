using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core.Extensions;
    using Core;
    using Core.Tokens;
    using Core.WordClasses;

    public class Evaluator : IEvaluator
    {
        public ScoreTreeNode ScoringTree;
        protected IList<Arranger> Arrangers = new List<Arranger>();

        private static readonly Lazy<ScoreTreeNode> LoadedScoringTree =
            new Lazy<ScoreTreeNode>(() => BuildScoringTree(Repository.LoadScoredPatterns()));

        private static readonly Lazy<IList<Arranger>> LoadedArrangers =
            new Lazy<IList<Arranger>>(() => BuildArrangers(Repository.LoadArrangements()));

        public Evaluator(IDictionary<string, sbyte> patterns = null)
        {
            ScoringTree = BuildScoringTree(patterns ?? new Dictionary<string, sbyte>());
        }

        public void Load()
        {
            ScoringTree = LoadedScoringTree.Value;
            Arrangers = LoadedArrangers.Value;
        }

        public Evaluation Evaluate(ushort[] code)
        {
            var mergedCode = MergeConjunctions(code);
            var scorings = GetMatchingScoreNodes(mergedCode)
                .GroupBy(n => n)
                .Select(n => new Scoring(n.Key.Path, (byte)n.Count(), n.Key.Score))
                .ToArray();
            return new Evaluation(scorings);
        }

        public Translation[] Arrange(IEnumerable<Translation> translations)
            => Arrangers
                .Aggregate(translations
                    , (input, arranger) => arranger
                        .Arrange(input.ToList()))
                .ToArray();

        private static IList<Arranger> BuildArrangers(IEnumerable<Arrangement> arrangements)
            => arrangements.Select(arr => new Arranger(arr)).ToList();

        private IEnumerable<ScoreTreeNode> GetMatchingScoreNodes(ushort[] code)
            => Enumerable.Range(0, code.Length)
                .SelectMany(i => GetMatchingScorers(ScoringTree, code, i));

        private static IEnumerable<ScoreTreeNode> GetMatchingScorers(ScoreTreeNode subtree, ushort[] code, int index)
        {
            if (subtree.Score != 0) yield return subtree;
            if (index >= code.Length) yield break;
            var matchingChildren = subtree.Children.Where(child => Matches(code[index], child.Code));
            foreach (var node in GetMatchingScorers(matchingChildren, code, index + 1))
                yield return node;
        }

        private static bool Matches(ushort code, ushort pattern)
            => code == pattern || (code | (ushort)Modifier.Any) == pattern;

        private static IEnumerable<ScoreTreeNode> GetMatchingScorers(IEnumerable<ScoreTreeNode> matchingChildren, ushort[] code, int index)
            => matchingChildren
                .SelectMany(child => GetMatchingScorers(child, code, index));

        private static ushort[] MergeConjunctions(ushort[] code)
        {
            var nextConjunctionIndex = GetNextHomogeneousConjunctionIndex(code);
            if (nextConjunctionIndex < 0)
                return code;

            var mergedCode = new List<ushort>();
            var prevConjunctionIndex = 0;
            while (nextConjunctionIndex >= 0)
            {
                mergedCode.AddRange(code.Take(nextConjunctionIndex).Skip(prevConjunctionIndex));
                mergedCode.Add(code[nextConjunctionIndex]);
                prevConjunctionIndex = nextConjunctionIndex + 3;
                nextConjunctionIndex = GetNextHomogeneousConjunctionIndex(code, prevConjunctionIndex);
            }
            mergedCode.AddRange(code.Skip(prevConjunctionIndex));

            return mergedCode.ToArray();
        }

        private static int GetNextHomogeneousConjunctionIndex(IReadOnlyList<ushort> code, int offset = 0)
        {
            for (var i = offset; i < code.Count - 2; i++)
                if (IsHomogeneousConjunction(code[i], code[i + 1], code[i + 2]))
                    return i;
            return -1;
        }

        private static bool IsHomogeneousConjunction(ushort a, ushort c, ushort b)
            => c == Conjunction.Code && a == b;
        private static ScoreTreeNode BuildScoringTree(IDictionary<string, sbyte> patterns)
        {
            var path = new ushort[0];
            return new ScoreTreeNode(0, path, 0, BuildScoringNodes(EncodePatterns(patterns), path).ToList());
        }

        private static IEnumerable<(string, ushort[], sbyte)> EncodePatterns(IDictionary<string, sbyte> patterns)
            => patterns.Select(CreateCodeScore);

        private static (string, ushort[], sbyte) CreateCodeScore(KeyValuePair<string, sbyte> kvp)
            => CreateCodeScore(kvp.Key, kvp.Value);

        protected static (string, ushort[], sbyte) CreateCodeScore(string pattern, sbyte score)
            => (pattern, Encoder.Encode(Encoder.Deserialize(pattern)).ToArray(), score);

        private static IEnumerable<ScoreTreeNode> BuildScoringNodes(IEnumerable<(string, ushort[], sbyte)> codedPatterns, ushort[] path, int index = 0)
            => codedPatterns
                .Where(t => t.Item2.Length > index)
                .GroupBy(item => item.Item2[index])
                .Select(g => BuildScoringNode(g, path.Append(g.Key).ToArray(), index + 1));

        private static ScoreTreeNode BuildScoringNode(IGrouping<ushort, (string, ushort[], sbyte)> g, 
            ushort[] path, 
            int index)
            => new ScoreTreeNode(
                    g.Key,
                    path,
                    GetNodeScore(g, index),
                    BuildScoringNodes(g, path, index).ToList());

        private static sbyte GetNodeScore(IEnumerable<(string, ushort[], sbyte)> codeScores, int index)
            => codeScores.SingleOrDefault(v => v.Item2.Length == index).Item3;
    }
}