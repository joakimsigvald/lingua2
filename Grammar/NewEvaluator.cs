namespace Lingua.Grammar
{
    using Core;
    using System.Collections.Generic;
    using System.Linq;

    public class NewEvaluator : IEvaluator
    {
        public ReverseCodeScoreNode Patterns = new ReverseCodeScoreNode();

        public byte Horizon => 6;

        private NewEvaluator(IDictionary<string, sbyte>? patterns)
        {
            if (patterns != null)
                foreach (var kvp in patterns)
                    Patterns.Extend(GetReversedCode(kvp.Key), kvp.Value);
        }

        public int EvaluateReversed(ushort[] invertedCode) => Patterns.Evaluate(invertedCode);

        private static ushort[] GetReversedCode(string symbols)
            => Encoder.Encode(symbols).Reverse().ToArray();

        public static IEvaluator Load() => new NewEvaluator(Repository.LoadScoredPatterns());
        public static NewEvaluator Create(IDictionary<string, sbyte>? patterns = null) => new NewEvaluator(patterns);
    }
}