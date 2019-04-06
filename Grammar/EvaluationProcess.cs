using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    internal class EvaluationProcess
    {
        private IEvaluator _evaluator;
        private ITranslation[] _translations;
        private CodeCondenser _codeCondenser = new CodeCondenser();

        public EvaluationProcess(IEvaluator evaluator, ITranslation[] translations)
        {
            _evaluator = evaluator;
            _translations = translations;
        }

        internal ReductionResult Evaluate()
        {
            var reversedCodes = _codeCondenser.GetReversedCodes(_translations).ToArray();
            return new ReductionResult(_translations, 
                reversedCodes.Last(), 
                reversedCodes.Sum(_evaluator.ScorePatternsEndingWith));
        }
    }
}