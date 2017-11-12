using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public class Evaluation : IEvaluation, IEquatable<IEvaluation>
    {
        public Evaluation(IList<Scoring> scorings)
        {
            Score = scorings.Sum(s => s.TotalScore);
            Patterns = scorings.Select(s => s.Pattern).ToList();
        }

        public IList<ushort[]> Patterns { get; }
        public int Score{ get; }

        public override string ToString() => $"{string.Join(",", Patterns.Select(Encoder.Serialize))}:{Score}";

        public bool Equals(IEvaluation other)
            => other != null && other.Patterns.SequenceEqual(Patterns) && other.Score == Score;

        public override bool Equals(object obj)
            => Equals(obj as IEvaluation);

        public override int GetHashCode()
            => Patterns.Sum(p => p.Sum(c => c));
    }
}