using System.Collections.Generic;
using Lingua.Grammar;

namespace Lingua.Learning
{
    public class EvaluatorEnhancement
    {
        public EvaluatorEnhancement(IList<ScoredPattern> scoredPatterns, Arranger arranger = null)
        {
            ScoredPatterns = scoredPatterns;
            Arranger = arranger;
        }

        public IList<ScoredPattern> ScoredPatterns { get; }
        public Arranger Arranger { get; }
    }
}