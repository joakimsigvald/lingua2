using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    internal class EvaluationProcess
    {
        private readonly IEvaluator _evaluator;
        private readonly IGrammaton[] _grammatons;
        private readonly CodeCondenser _codeCondenser = new CodeCondenser();

        public EvaluationProcess(IEvaluator evaluator, IGrammaton[] grammatons)
        {
            _evaluator = evaluator;
            _grammatons = grammatons;
        }

        internal ReductionResult Evaluate()
        {
            var reversedCodes = _codeCondenser.GetReversedCodes(_grammatons).ToArray();
            return new ReductionResult(_grammatons, 
                reversedCodes.Last(), 
                reversedCodes.Sum(_evaluator.ScorePatternsEndingWith));
        }
    }
}