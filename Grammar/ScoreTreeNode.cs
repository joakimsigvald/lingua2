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
    }
}