using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
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
        private readonly PatternGenerator _patternGenerator;
        private TestRunner _testRunner;

        public Trainer()
        {
            _evaluator = new TrainableEvaluator();
            _tokenizer = new Tokenizer();
            _translator = new Translator(_tokenizer, new Thesaurus(), new Engine(_evaluator));
            _patternGenerator = new PatternGenerator(new PatternExtractor(), new TranslationExtractor());
        }

        public (TestCaseResult, int) RunTrainingSession(params TestCase[] testCases)
        {
            _testRunner = new TestRunner(_translator, _tokenizer, true);
            IEnumerator<(string, sbyte)> scoredPatterns = new List<(string, sbyte)>().GetEnumerator();
            var runTestsCount = 0;
            (string currentPattern, sbyte currentScore) = (null, 0);
            TestCaseResult[] results;
            TestCaseResult failedCase = null;
            while (!(results = _testRunner.RunTestCases(testCases)).All(result => result.Success))
            {
                var lastFailedCase = results.Last();
                if (results.Length > runTestsCount)
                {
                    failedCase = lastFailedCase;
                    (currentPattern, currentScore) = (null, 0);
                    scoredPatterns.Dispose();
                    scoredPatterns = EnumerateMatchingPatterns(failedCase);
                    runTestsCount = results.Length;
                }
                do
                {
                    _evaluator.UpdateScore(currentPattern, (sbyte)-currentScore);
                    if (!scoredPatterns.MoveNext())
                        return (failedCase, runTestsCount);
                    (currentPattern, currentScore) = scoredPatterns.Current;
                    _evaluator.UpdateScore(currentPattern, currentScore);
                    lastFailedCase = _testRunner.RunTestCase(lastFailedCase.TestCase);
                } while (!lastFailedCase.Success);
            }
            scoredPatterns.Dispose();
            return (null, runTestsCount);
        }

        private IEnumerator<(string, sbyte)> EnumerateMatchingPatterns(TestCaseResult result)
            => _patternGenerator.GetMatchingPatterns(result).GetEnumerator();
    }
}