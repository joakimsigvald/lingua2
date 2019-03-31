using System.Collections.Generic;

namespace Lingua.Grammar
{
    using Core;
    using System.Linq;

    public class GrammarEngine : IGrammar
    {
        private readonly IEvaluator _evaluator;
        public GrammarEngine(IEvaluator evaluator) => _evaluator = evaluator;

        public ReductionResult Reduce(IList<ITranslation[]> possibilities)
            => possibilities.Any()
            ? new ReductionProcess(_evaluator, possibilities.ToArray()).Reduce()
            : new ReductionResult();

        public int Evaluate(ITranslation[] translations)
            => translations.Any()
            ? new EvaluationProcess(_evaluator, translations).Evaluate()
            : 0;
    }
}