using System.Collections.Generic;

namespace Lingua.Grammar
{
    using Core;
    using System.Linq;

    public class GrammarEngine : IGrammar
    {
        private readonly IEvaluator _evaluator;
        public GrammarEngine(IEvaluator evaluator) => _evaluator = evaluator;

        public ReductionResult Reduce(IDecomposition decomposition)
            => decomposition.IsEmpty
            ? ReductionResult.Empty
            : new ReductionProcess(_evaluator, decomposition).Reduce();

        public ReductionResult Evaluate(IGrammaton[] grammatons)
            => grammatons.Any()
            ? new EvaluationProcess(_evaluator, grammatons).Evaluate()
            : ReductionResult.Empty;
    }
}