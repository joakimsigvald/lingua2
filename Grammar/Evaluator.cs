namespace Lingua.Grammar
{
    using Core;
    using System.Collections.Generic;

    public class Evaluator : IEvaluator
    {
        public ReverseCodeScoreNode Patterns { get; } = new ReverseCodeScoreNode();

        public byte Horizon { get; } = Constants.Horizon;

        private Evaluator(IDictionary<string, sbyte>? patterns)
        {
            if (patterns != null)
                foreach (var kvp in patterns)
                    Patterns.Extend(GetReversedCode(kvp.Key), kvp.Value);
        }

        private Evaluator(IDictionary<Code, sbyte> patterns)
        {
            foreach (var kvp in patterns)
                Patterns.Extend(kvp.Key.ReversedCode, kvp.Value);
        }

        public int ScorePatternsEndingWith(ushort[] reversedCode) => Patterns.Evaluate(reversedCode);

        private static ushort[] GetReversedCode(string symbols)
            => Encoder.Encode(symbols).ReversedCode;

        public static IEvaluator Load() => new Evaluator(Repository.LoadScoredPatterns());
        public static Evaluator Create(IDictionary<string, sbyte>? patterns = null) => new Evaluator(patterns);
        public static Evaluator Create(IDictionary<Code, sbyte> patterns) => new Evaluator(patterns);
    }
}