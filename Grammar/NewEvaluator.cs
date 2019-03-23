using System;

namespace Lingua.Grammar
{
    using Core;
    using System.Collections.Generic;
    using System.Linq;

    public class NewEvaluator : IEvaluator
    {
        public ReverseCodeScoreNode Patterns = new ReverseCodeScoreNode();
        private static readonly Lazy<ReverseCodeScoreNode> LoadedPatterns =
            new Lazy<ReverseCodeScoreNode>(() => BuildPatterns(Repository.LoadScoredPatterns()));

        private static ReverseCodeScoreNode BuildPatterns(IDictionary<string, sbyte> patterns)
        {
            var root = new ReverseCodeScoreNode();
            foreach (var kvp in patterns)
                root.Extend(GetReversedCode(kvp.Key), kvp.Value);
            return root;
        }

        public byte Horizon => 6;

        public NewEvaluator(IDictionary<string, sbyte>? patterns = null)
        {
            if (patterns != null)
                foreach (var kvp in patterns)
                    Patterns.Extend(GetReversedCode(kvp.Key), kvp.Value);
        }

        public IEvaluation Evaluate(ushort[] code, int commonLength = 0)
        {
            throw new NotImplementedException();
        }

        public int EvaluateReversed(ushort[] invertedCode) => Patterns.Evaluate(invertedCode);

        private static ushort[] GetReversedCode(string symbols)
            => Encoder.Encode(symbols).Reverse().ToArray();

        public void Load()
        {
            Patterns = LoadedPatterns.Value;
        }
    }
}