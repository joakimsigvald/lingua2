using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;

namespace Lingua.Grammar
{
    public class Evaluator
    {
        private static readonly IDictionary<string, int> StoredPatterns 
            = Loader.LoadScoredPatterns();

        private readonly IList<Scorer> _scorers;

        public Evaluator(IDictionary<string, int> patterns = null)
        {
            patterns = patterns ?? StoredPatterns;
            _scorers = patterns.Select(sp => new Scorer
            {
                Pattern = Encoder.Encode(Encoder.Deserialize(sp.Key)).ToArray(),
                Score = sp.Value
            }).ToList();
        }

        public Evaluation Evaluate(ushort[] code)
        {
            var mergedCode = MergeConjunctions(code).Prepend(Start.Code).ToArray();
            var scorings = _scorers.Select(s
                    => new Scoring(s.Pattern, s.CountMatches(mergedCode), s.Score))
                .Where(m => m.Count > 0)
                .ToArray();
            return new Evaluation(scorings);
        }

        private static IEnumerable<ushort> MergeConjunctions(ushort[] code)
        {
            if (code.Length < 3)
                return code;
            (var a, var b, var c) = code.Take(3);
            var isHomogeneous = IsHomogeneousConjunction(a, b, c);
            return isHomogeneous
                ? MergeConjunctions(code.Skip(3).ToArray()).Prepend(a)
                : MergeConjunctions(code.Skip(1).ToArray()).Prepend(a);
        }

        private static bool IsHomogeneousConjunction(ushort a, ushort c, ushort b)
           => c == Conjunction.Code && a == b;
    }
}