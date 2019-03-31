namespace Lingua.Grammar
{
    using Core;
    using System.Collections.Generic;
    using System.Linq;

    public class Evaluator : IEvaluator
    {
        public ReverseCodeScoreNode Patterns = new ReverseCodeScoreNode();

        public byte Horizon => 6;

        private Evaluator(IDictionary<string, sbyte>? patterns)
        {
            if (patterns != null)
                foreach (var kvp in patterns)
                    Patterns.Extend(GetReversedCode(kvp.Key), kvp.Value);
        }

        public int ScorePatternsEndingWith(ushort[] reversedCode) => Patterns.Evaluate(reversedCode);

        private static ushort[] GetReversedCode(string symbols)
            => Encoder.Encode(symbols).Reverse().ToArray();

        public static IEvaluator Load() => new Evaluator(Repository.LoadScoredPatterns());
        public static Evaluator Create(IDictionary<string, sbyte>? patterns = null) => new Evaluator(patterns);
    }
}