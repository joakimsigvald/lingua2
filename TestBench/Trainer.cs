using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    using Core;
    using Grammar;
    using Tokenization;
    using Vocabulary;

    public class Trainer
    {
        private readonly TrainableEvaluator _evaluator;
        private readonly ITranslator _translator;
        private TestRunner _testRunner;

        public Trainer()
        {
            _evaluator = new TrainableEvaluator();
            _translator = new Translator(new Tokenizer(), new Thesaurus(), new Engine(_evaluator));
        }

        public bool RunTrainingSession(int caseCount)
        {
            _testRunner = new TestRunner(_translator, caseCount, true);
            IEnumerator<string> addPatterns = new List<string>().GetEnumerator();
            var runTestsCount = 0;
            string currentPattern = null;
            TestCaseResult[] results;
            while (!(results = _testRunner.RunTestCases()).All(result => result.Success))
            {
                var failedCase = results.Last();
                if (results.Length > runTestsCount)
                {
                    currentPattern = null;
                    addPatterns.Dispose();
                    addPatterns = GetPossiblePatternsToAdd(failedCase);
                    runTestsCount = results.Length;
                }
                do
                {
                    _evaluator.RemovePattern(currentPattern);
                    if (!addPatterns.MoveNext())
                        return false;
                    currentPattern = addPatterns.Current;
                    _evaluator.AddPattern(currentPattern);
                    failedCase = _testRunner.RunTestCase(failedCase.Group, failedCase.From,
                        failedCase.Expected);
                } while (!failedCase.Success);
            }
            addPatterns.Dispose();
            return true;
        }

        private IEnumerator<string> GetPossiblePatternsToAdd(TestCaseResult result)
        {
            throw new System.NotImplementedException();
        }
    }
}