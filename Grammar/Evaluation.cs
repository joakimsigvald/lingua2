using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    public class Evaluation : IEvaluation
    {
        public Evaluation(IList<Scoring> scorings)
        {
            Score = scorings.Sum(s => s.TotalScore);
            Patterns = scorings.Select(s => s.Pattern).ToList();
        }

        public IList<ushort[]> Patterns { get; }
        public int Score{ get; }

        public override string ToString() => $"{string.Join(",", Patterns)}:{Score}";
    }
}