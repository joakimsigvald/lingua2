using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Extensions;

namespace Lingua.Grammar
{
    using Core;

    public class ScoreTreeNode
    {
        public ScoreTreeNode(ushort code, ushort[] path, sbyte score, IEnumerable<ScoreTreeNode> children)
        {
            Code = code;
            _classCode = Encoder.GetClassCode(code);
            Path = path;
            Score = score;
            Children = children.OrderBy(c => c._classCode).ToArray();
        }

        public ScoreTreeNode[] Children;
        public readonly ushort Code;
        private readonly ushort _classCode;
        public readonly ushort[] Path;
        public sbyte Score { get; set; }

        public IEnumerable<ushort[]> Patterns => 
            Children.Any() 
            ? Children
            .SelectMany(child => child.Patterns)
            .Select(pattern => pattern.Prepend(Code).ToArray())
            : new [] { new[] { Code } };

        private string Pattern => Encoder.Serialize(Path);

        public override string ToString()
            => string.Join(Environment.NewLine, PatternLines);

        public string[] PatternLines
            => ToDictionary()
                .OrderByDescending(sp => sp.Value)
                .ThenBy(sp => sp.Key)
                .Select(ToLine).ToArray();

        private static string ToLine(KeyValuePair<string, sbyte> scoredPattern)
            => $"{scoredPattern.Key}:{scoredPattern.Value}";

        private IDictionary<string, sbyte> ToDictionary() 
            => GetScoredPatterns().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        private IEnumerable<KeyValuePair<string, sbyte>> GetScoredPatterns()
        {
            var scoredPatterns = Children.SelectMany(child => child.ToDictionary());
            return Score == 0
                ? scoredPatterns
                : scoredPatterns.Prepend(new KeyValuePair<string, sbyte>(Pattern, Score));
        }

        public IEnumerable<ScoreTreeNode> GetMatchingChildren(ushort code)
        {
            var classCode = Encoder.GetClassCode(code);
            return Children.Where(child => child._classCode == classCode && Encoder.Matches(code, child.Code));
        }

        public void AddChild(ScoreTreeNode child)
        {
            Children = Children.Append(child).OrderBy(c => c._classCode).ToArray();
        }

        public void RemoveChild(ScoreTreeNode child)
        {
            Children = Children.Except(child).ToArray();
        }

        // TODO: 
        // generate new patterns with patterns replaced with aggregate code, and same score
        // remove old patterns
        // add new patterns
        // automatically remove subtree without children or code
        // replace aggregate pattern with aggregate code in generated sequences
        // extend sequences with aggregates backwards to full attention span
        // save and load aggrgegates, initialize next aggregate code
        // add test case: He took the ball with the blue dot and kicked it + other testcases with [TdA*Nd]
        public void Replace(ushort[] aggregatePattern, ushort aggregateCode)
        {
            var matchingPatterns = Patterns.Where(pattern => Contains(pattern, aggregatePattern));
        }

        private static bool Contains(ushort[] pattern, ushort[] aggregatePattern)
            => pattern.Length >= aggregatePattern.Length
               && (StartsWith(pattern, aggregatePattern) || Contains(pattern.Skip(1).ToArray(), aggregatePattern));

        private static bool StartsWith(ushort[] x, ushort[] y) 
            => x.Length >= y.Length && !y.Where((t, i) => x[i] != t).Any();
    }
}