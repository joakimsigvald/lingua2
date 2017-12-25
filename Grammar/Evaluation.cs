using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lingua.Grammar
{
    using Core;

    public class Evaluation : IEvaluation, IEquatable<IEvaluation>
    {
        private readonly Lazy<string> _code;

        public Evaluation(IList<Scoring> scorings)
        {
            Score = scorings.Sum(s => s.TotalScore);
            Patterns = scorings.Select(s => s.Pattern).ToArray();
            _code = new Lazy<string>(MakeCode);
        }

        private string MakeCode()
        {
            var sb = new StringBuilder();
            sb.Append((char)Score);
            int i = 1;
            int j;
            foreach (var pattern in Patterns)
            {
                j = 0;
                for (; j < pattern.Length - 1; j += 2)
                    sb.Append((char) (pattern[j] + (pattern[j + 1] << 16)));
                if (j < pattern.Length)
                    sb[0] = (char) (pattern[j] << 16);
            }
            return sb.ToString();
        }

        public IList<ushort[]> Patterns { get; }
        public int Score{ get; }

        public override string ToString() => $"{string.Join(",", Patterns.Select(Encoder.Serialize))}:{Score}";

        public bool Equals(IEvaluation other)
            => other != null && ((Evaluation)other)._code.Value == _code.Value;

        public override bool Equals(object obj)
            => Equals(obj as IEvaluation);

        public override int GetHashCode()
            => _code.Value.GetHashCode();
    }
}