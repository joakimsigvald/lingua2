using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;

namespace Lingua.Grammar
{
    using Core;

    public class Evaluator
    {
        private static readonly IDictionary<string, int> StoredPatterns 
            = Loader.LoadScoredPatterns();

        private readonly ScoreTreeNode _scoringTree;

        public Evaluator(IDictionary<string, int> patterns = null)
        {
            patterns = patterns ?? StoredPatterns;
            _scoringTree = BuildScoringTree(patterns);
        }

        private static ScoreTreeNode BuildScoringTree(IDictionary<string, int> patterns)
        {
            var path = new ushort[0];
            return new ScoreTreeNode(0, path, null, BuildScoringNodes(EncodePatterns(patterns), path).ToArray());
        }

        private static IEnumerable<Tuple<string, ushort[], sbyte>> EncodePatterns(IDictionary<string, int> patterns)
            => patterns.Select(CreateCodeScore);

        private static Tuple<string, ushort[], sbyte> CreateCodeScore(KeyValuePair<string, int> kvp)
            => new Tuple<string, ushort[], sbyte>(kvp.Key, Encoder.Encode(Encoder.Deserialize(kvp.Key)).ToArray(), (sbyte)kvp.Value);

        private static IEnumerable<ScoreTreeNode> BuildScoringNodes(IEnumerable<Tuple<string, ushort[], sbyte>> codedPatterns, ushort[] path, int index = 0)
        {
            return codedPatterns
                .Where(t => t.Item2.Length > index)
                .GroupBy(item => item.Item2[index])
                .Select(g => BuildScoringNode(g, path, index + 1));
        }

        private static ScoreTreeNode BuildScoringNode(IGrouping<ushort, Tuple<string, ushort[], sbyte>> g, 
            IEnumerable<ushort> parentPath, 
            int index)
        {
            var path = parentPath.Append(g.Key).ToArray();
            return new ScoreTreeNode(
                    g.Key,
                    path,
                    GetNodeScore(g, index),
                    BuildScoringNodes(g, path, index).ToArray());
        }

        private static sbyte? GetNodeScore(IEnumerable<Tuple<string, ushort[], sbyte>> codeScores, int index)
            => codeScores.SingleOrDefault(v => v.Item2.Length == index)?.Item3;

        public Evaluation Evaluate(ushort[] code)
        {
            var mergedCodes = MergeConjunctions(code);
            var scorings = GetMatchingScoreNodes(mergedCodes)
                .GroupBy(n => n)
                .Select(n => new Scoring(n.Key.Path, (byte)n.Count(), n.Key.Score ?? 0))
                .ToArray();
            return new Evaluation(scorings);
        }

        private IEnumerable<ScoreTreeNode> GetMatchingScoreNodes(ushort[] code)
            => Enumerable.Range(0, code.Length)
                .SelectMany(i => GetMatchingScorers(_scoringTree, code, i));

        private static IEnumerable<ScoreTreeNode> GetMatchingScorers(ScoreTreeNode subtree, ushort[] code, int index)
        {
            if (subtree.Score.HasValue) yield return subtree;
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
    }
}