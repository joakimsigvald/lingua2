using System;

namespace Lingua.Grammar
{
    using Core;
    using System.Collections.Generic;
    using System.Linq;

    public class ReverseCodeScoreNode
    {
        public List<ReverseCodeScoreNode> Previous { get; } = new List<ReverseCodeScoreNode>();
        public ushort Code { get; }
        public sbyte Score { get; set; }

        public ReverseCodeScoreNode(ushort code = 0) => Code = code;

        public void Extend(ushort[] reversedCode, sbyte score, int index = 0)
        {
            if (index == reversedCode.Length)
            {
                if (Score != 0)
                    throw new InvalidOperationException("Pattern is already registered:");
                Score = score;
                return;
            }
            var first = reversedCode[index];
            var previous = Previous.SingleOrDefault(p => p.Code == first);
            if (previous == null)
                Previous.Add(previous = new ReverseCodeScoreNode(first));
            previous.Extend(reversedCode, score, index + 1);
        }

        internal int Evaluate(ushort[] invertedCode, int index = 0)
        {
            if (index == invertedCode.Length)
                return Score;
            var first = invertedCode[index++];
            var matches = Previous.Where(p => Encoder.Matches(first, p.Code));
            return Score + matches.Sum(m => m.Evaluate(invertedCode, index));
        }

        public string[] PatternLines
            => Previous.SelectMany(p => p.GetScoredPatterns(new ushort[0]))
                .OrderByDescending(sp => sp.Value)
                .ThenBy(sp => sp.Key)
                .Select(ToLine).ToArray();

        private IEnumerable<KeyValuePair<string, sbyte>> GetScoredPatterns(ushort[] rest)
        {
            var sequence = rest.Prepend(Code).ToArray();
            var scoredPatterns = Previous.SelectMany(child => child.GetScoredPatterns(sequence));
            return Score == 0
                ? scoredPatterns
                : scoredPatterns.Prepend(new KeyValuePair<string, sbyte>(Encoder.Serialize(sequence), Score));
        }

        private static string ToLine(KeyValuePair<string, sbyte> scoredPattern)
            => $"{scoredPattern.Key}:{scoredPattern.Value}";
    }
}