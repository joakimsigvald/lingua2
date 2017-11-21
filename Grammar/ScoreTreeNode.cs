﻿using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public class ScoreTreeNode
    {
        public ScoreTreeNode(ushort code, ushort[] path, sbyte score, List<ScoreTreeNode> children)
        {
            Code = code;
            Path = path;
            Score = score;
            Children = children;
        }

        public readonly List<ScoreTreeNode> Children;
        public readonly ushort Code;
        public readonly ushort[] Path;
        public sbyte Score { get; set; }

        public int ScoredNodeCount => (Score == 0 ? 0 : 1) + Children.Sum(child => child.ScoredNodeCount);
        private string Pattern => Encoder.Serialize(Path);

        public override string ToString()
            => $"{Pattern}: {Score}";

        public IDictionary<string, sbyte> ToDictionary() 
            => GetScoredPatterns().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        private IEnumerable<KeyValuePair<string, sbyte>> GetScoredPatterns()
        {
            var scoredPatterns = Children.SelectMany(child => child.ToDictionary());
            return Score == 0
                ? scoredPatterns
                : scoredPatterns.Prepend(new KeyValuePair<string, sbyte>(Pattern, Score));
        }
    }
}