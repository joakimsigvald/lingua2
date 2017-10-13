using System.Collections.Generic;
using System.Linq;
using Lingua.Core.WordClasses;

namespace Lingua.Grammar
{
    using Core;

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
            var mergedCodes = MergeConjunctions(code);
            var scorings = _scorers.Select(s
                    => new Scoring(code, s.Pattern, s.CountMatches(mergedCodes), s.Score))
                .Where(m => m.Count > 0)
                .ToArray();
            return new Evaluation(scorings);
        }

        private static ushort[] MergeConjunctions(ushort[] code)
        {
            var nextConjunctionIndex = GetNextHomogeneousConjunctionIndex(code);
            if (nextConjunctionIndex < 0)
                return code;

            var mergedCode = new List<ushort>();
            var prevConjunctionIndex = 0;
            while (nextConjunctionIndex >= 0)
            {
                mergedCode.AddRange(code.Take(nextConjunctionIndex).Skip(prevConjunctionIndex));
                mergedCode.Add(code[nextConjunctionIndex]);
                prevConjunctionIndex = nextConjunctionIndex + 3;
                nextConjunctionIndex = GetNextHomogeneousConjunctionIndex(code, prevConjunctionIndex);
            }
            mergedCode.AddRange(code.Skip(prevConjunctionIndex));

            return mergedCode.ToArray();
        }

        private static int GetNextHomogeneousConjunctionIndex(IReadOnlyList<ushort> code, int offset = 0)
        {
            for (var i = offset; i < code.Count - 2; i++)
                if (IsHomogeneousConjunction(code[i], code[i + 1], code[i + 2]))
                    return i;
            return -1;
        }

        private static bool IsHomogeneousConjunction(ushort a, ushort c, ushort b)
            => c == Conjunction.Code && a == b;
    }
}