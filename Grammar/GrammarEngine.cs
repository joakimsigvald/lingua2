using System.Collections.Generic;

namespace Lingua.Grammar
{
    using Core;
    using System.Linq;

    public class GrammarEngine : IGrammar
    {
        private readonly IEvaluator _evaluator;
        public GrammarEngine(IEvaluator evaluator) => _evaluator = evaluator;

        public ReductionResult Reduce(IList<IGrammaton[]> possibilities)
            => possibilities.Any()
            ? new ReductionProcess(_evaluator, possibilities.ToArray()).Reduce()
            : new ReductionResult();

        public ReductionResult Evaluate(IGrammaton[] grammatons)
            => grammatons.Any()
            ? new EvaluationProcess(_evaluator, grammatons).Evaluate()
            : new ReductionResult();
    }
}