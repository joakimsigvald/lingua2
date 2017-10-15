using System.Collections.Generic;
using Lingua.Grammar;

namespace Lingua.Testing
{
    public class TrainableEvaluator : IEvaluator
    {
        private readonly Evaluator _evaluator;

        public TrainableEvaluator(IDictionary<string, int> patterns = null) 
            => _evaluator = new Evaluator(patterns);

        public Evaluation Evaluate(ushort[] code)
            => _evaluator.Evaluate(code);

        public void AddPattern(string addPatternsCurrent)
        {
            throw new System.NotImplementedException();
        }

        public void RemovePattern(string currentPattern)
        {
            throw new System.NotImplementedException();
        }
    }
}