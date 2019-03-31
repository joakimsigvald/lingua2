using System;
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
            var score = 0;
            ushort[] previousReversed = new ushort[0];
            foreach (ITranslation t in _translations)
            {
                previousReversed = _codeCondenser.ReplaceLastNounPhrase(previousReversed.Prepend(t.Code).ToArray());
                score += _evaluator.ScorePatternsEndingWith(previousReversed);
            }
            return new ReductionResult(_translations, previousReversed, score);
        }
    }
}