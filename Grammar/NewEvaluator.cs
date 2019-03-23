using System;

namespace Lingua.Grammar
{
    using Core;
    using System.Collections.Generic;
    using System.Linq;

    public class NewEvaluator : IEvaluator
    {
        private Pattern _patterns = new Pattern();
        private static readonly Lazy<Pattern> LoadedPatterns =
            new Lazy<Pattern>(() => BuildPatterns(Repository.LoadScoredPatterns()));

        private static Pattern BuildPatterns(IDictionary<string, sbyte> patterns)
        {
            var root = new Pattern();
            foreach (var kvp in patterns)
                root.Extend(GetReversedCode(kvp.Key), kvp.Value);
            return root;
        }

        public byte Horizon => 6;

        public NewEvaluator(IDictionary<string, sbyte>? patterns = null)
        {
            if (patterns != null)
                foreach (var kvp in patterns)
                    _patterns.Extend(GetReversedCode(kvp.Key), kvp.Value);
        }

        public IEvaluation Evaluate(ushort[] code, int commonLength = 0)
        {
            throw new NotImplementedException();
        }

        public int EvaluateReversed(ushort[] invertedCode) => _patterns.Evaluate(invertedCode);

        private static ushort[] GetReversedCode(string symbols)
            => Encoder.Encode(symbols).Reverse().ToArray();

        public void Load()
        {
            _patterns = LoadedPatterns.Value;
        }

        private class Pattern
        {
            private List<Pattern> _previous = new List<Pattern>();
            private readonly ushort _code;
            private sbyte _score;

            internal Pattern(ushort code = 0) => _code = code;

            internal void Extend(ushort[] inversedCode, sbyte score)
            {
                if (!inversedCode.Any())
                {
                    if (_score != 0)
                        throw new InvalidOperationException("Pattern is already registered:");
                    _score = score;
                    return;
                }
                var first = inversedCode[0];
                var rest = inversedCode.Skip(1).ToArray();
                var previous = _previous.SingleOrDefault(p => p._code == first);
                if (previous == null)
                    _previous.Add(previous = new Pattern(first));
                previous.Extend(rest, score);
            }

            internal int Evaluate(ushort[] invertedCode)
            {
                if (!invertedCode.Any())
                    return _score;
                var first = invertedCode[0];
                var rest = invertedCode.Skip(1).ToArray();
                var matches = _previous.Where(p => Encoder.Matches(first, p._code));
                return _score + matches.Sum(m => m.Evaluate(rest));
            }
        }
    }
}