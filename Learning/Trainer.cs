using System.Collections.Generic;

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
            var result = LearnPatterns(testCases);
            if (!result.Success)
                return result;
            LearnRearrangements();
            return VerifyPatterns(testCases);
        }

        private void LearnRearrangements()
        {
            _evaluator.LoadRearrangements();
        }

        private TestSessionResult LearnPatterns(TestCase[] testCases)
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
                    }
                    else scoredPatterns.Reset();
                    bestResult = result;
                }
                var lastFailedCase = result.FailedCase;
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
            scoredPatterns.Dispose();
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