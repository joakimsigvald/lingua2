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

        public Trainer()
        {
            _evaluator = new TrainableEvaluator();
            _tokenizer = new Tokenizer();
            _translator = new Translator(_tokenizer, new Thesaurus(), new GrammarEngine(_evaluator));
            _patternGenerator = new PatternGenerator(new TranslationExtractor(), new PatternExtractor());
        }

        public TestSessionResult RunTrainingSession(params TestCase[] testCases)
        {
            var copyOfTestCases = testCases.ToList();
            var result = LearnPatterns(copyOfTestCases);
            if (!result.Success)
                return result;
            LearnRearrangements();
            return VerifyPatterns(testCases);
        }

        private void LearnRearrangements()
        {
            _evaluator.LoadRearrangements();
        }

        private TestSessionResult LearnPatterns(IList<TestCase> testCases)
        {
            var settings = new TestRunnerSettings
            {
                AbortOnFail = true,
                AllowReordered = true
            };
            var testRunner = new TestRunner(_translator, _tokenizer, _evaluator, settings);
            IEnumerator<ScoredPattern> scoredPatterns = null;
            TestSessionResult bestResult = null;
            ScoredPattern currentScoredPattern = null;
            TestSessionResult result;
            TestCaseResult lastFailedCase = null;
            while (!(result = testRunner.RunTestCases(testCases)).Success)
            {
                if (bestResult == null)
                {
                    bestResult = result;
                    scoredPatterns = EnumerateScoredPatterns(result.FailedCase);
                }
                else if (result > bestResult)
                {
                    currentScoredPattern = null;
                    if (result.SuccessCount > bestResult.SuccessCount)
                    {
                        scoredPatterns.Dispose();
                        scoredPatterns = EnumerateScoredPatterns(result.FailedCase);
                        testCases.MoveToBeginning(lastFailedCase.TestCase);
                    }
                    else scoredPatterns.Reset();
                    bestResult = result;
                }
                lastFailedCase = result.FailedCase;
                if (result.SuccessCount < bestResult.SuccessCount)
                    testCases.MoveToBeginning(lastFailedCase.TestCase);
                do
                {
                    if (currentScoredPattern != null)
                        _evaluator.Undo(currentScoredPattern);
                    if (!scoredPatterns.MoveNext())
                        return bestResult;
                    currentScoredPattern = scoredPatterns.Current;
                    _evaluator.Do(currentScoredPattern);
                    lastFailedCase = testRunner.RunTestCase(lastFailedCase.TestCase);
                } while (lastFailedCase.ScoreDeficit >= bestResult.FailedCase.ScoreDeficit);
            }
            return result;
        }

        private TestSessionResult VerifyPatterns(IEnumerable<TestCase> testCases)
        {
            var settings = new TestRunnerSettings
            {
                AbortOnFail = true,
                AllowReordered = false
            };
            var testRunner = new TestRunner(_translator, _tokenizer, _evaluator, settings);
            return testRunner.RunTestCases(testCases);
        }

        public void SavePatterns()
        {
            _evaluator.SavePatterns();
        }

        private IEnumerator<ScoredPattern> EnumerateScoredPatterns(TestCaseResult result)
            => _patternGenerator.GetScoredPatterns(result).GetEnumerator();
    }
}