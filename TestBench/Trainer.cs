using System.Collections.Generic;
using Lingua.Core;
using Lingua.Grammar;
using Lingua.Tokenization;
using Lingua.Vocabulary;

namespace Lingua.Testing
{
    public class Trainer : IReporter
    {
        private readonly TrainableEvaluator _evaluator;
        private readonly ITranslator _translator;
        private TestBench _testBench;

        public Trainer()
        {
            _evaluator = new TrainableEvaluator();
            _translator = new Translator(new Tokenizer(), new Thesaurus(), new Engine(_evaluator));
        }

        public bool RunTrainingSession(int caseCount)
        {
            _testBench = new TestBench(_translator, this, caseCount, true);
            while (!_testBench.RunTestSuites())
            {

            }
            return false;
        }

        public void Report(IEnumerable<TestCaseResult> testCaseResults)
        {
            

        }
    }
}