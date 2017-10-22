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
        private readonly ITokenizer _tokenizer;
        private TestRunner _testRunner;

        public Trainer()
        {
            _evaluator = new TrainableEvaluator();
            _tokenizer = new Tokenizer();
            _translator = new Translator(_tokenizer, new Thesaurus(), new Engine(_evaluator));
        }

        public (TestCaseResult, int) RunTrainingSession(params TestCase[] testCases)
        {
            _testRunner = new TestRunner(_translator, _tokenizer, true);
            IEnumerator<string> matchingPatterns = new List<string>().GetEnumerator();
            var runTestsCount = 0;
            string currentPattern = null;
            TestCaseResult[] results;
            TestCaseResult failedCase = null;
            while (!(results = _testRunner.RunTestCases(testCases)).All(result => result.Success))
            {
                var lastFailedCase = results.Last();
                if (results.Length > runTestsCount)
                {
                    failedCase = lastFailedCase;
                    currentPattern = null;
                    matchingPatterns.Dispose();
                    matchingPatterns = EnumerateMatchingPatterns(failedCase);
                    runTestsCount = results.Length;
                }
                do
                {
                    _evaluator.DownPattern(currentPattern);
                    if (!matchingPatterns.MoveNext())
                        return (failedCase, runTestsCount);
                    currentPattern = matchingPatterns.Current;
                    _evaluator.UpPattern(currentPattern);
                    lastFailedCase = _testRunner.RunTestCase(lastFailedCase.TestCase);
                } while (!lastFailedCase.Success);
            }
            matchingPatterns.Dispose();
            return (null, runTestsCount);
        }

        private static IEnumerator<string> EnumerateMatchingPatterns(TestCaseResult result)
            => PatternGenerator.GetMatchingPatterns(result).GetEnumerator();
    }
}