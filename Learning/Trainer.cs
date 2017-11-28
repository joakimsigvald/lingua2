using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Extensions;

namespace Lingua.Learning
{
    using Core;
    using Grammar;
    using Tokenization;
    using Vocabulary;

    public class Trainer
    {
        private readonly TrainableEvaluator _evaluator;
        private readonly Translator _translator;
        private readonly ITokenizer _tokenizer;
        private readonly IGrammar _grammar;
        private readonly PatternGenerator _patternGenerator;

        public Trainer()
        {
            _evaluator = new TrainableEvaluator();
            _tokenizer = new Tokenizer();
            _grammar = new GrammarEngine(_evaluator);
            _translator = new Translator(_tokenizer, new Thesaurus(), _grammar);
            _patternGenerator = new PatternGenerator(new TranslationExtractor(), new PatternExtractor());
        }

        public TestSessionResult RunTrainingSession(params TestCase[] testCases)
        {
            var preparedTestCases = PrepareForLearning(testCases);
            var result = LearnPatterns(preparedTestCases);
            if (!result.Success)
                return result;
            LearnRearrangements();
            return VerifyPatterns(testCases);
        }

        private IList<TestCase> PrepareForLearning(TestCase[] testCases)
        {
            var preparedTestCases = testCases
                .Where(tc => !string.IsNullOrWhiteSpace(tc.From))
                .ToList();
            preparedTestCases.ForEach(PrepareForLearning);
            return preparedTestCases;
        }

        private void PrepareForLearning(TestCase testCase)
        {
            testCase.Possibilities = _translator.Destruct(testCase.From);
            testCase.Target = TargetSelector.SelectTarget(testCase.Possibilities, testCase.Expected);
            if (testCase.Target.Translations == null)
                throw new Exception();
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
            var testRunner = new TestRunner(new WordByWordTranslator(_grammar), _evaluator, settings);
            IEnumerator<ScoredPattern> scoredPatterns = null;
            TestSessionResult bestResult = null;
            ScoredPattern currentScoredPattern = null;
            TestSessionResult result;
            TestCaseResult lastFailedCase = null;
            while (!(result = testRunner.RunTestSession(testCases)).Success)
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
                        testRunner.KnownResult = null;
                    }
                    else scoredPatterns.Reset();
                    bestResult = result;
                }
                lastFailedCase = result.FailedCase;
                do
                {
                    if (currentScoredPattern != null)
                        _evaluator.Undo(currentScoredPattern);
                    if (!scoredPatterns.MoveNext())
                        return bestResult;
                    currentScoredPattern = scoredPatterns.Current;
                    _evaluator.Do(currentScoredPattern);
                    testRunner.KnownResult = null;
                    lastFailedCase = testRunner.RunTestCase(lastFailedCase.TestCase);
                } while (lastFailedCase.ScoreDeficit >= bestResult.FailedCase.ScoreDeficit);
                if (lastFailedCase.IsSuccess)
                    testCases.MoveToBeginning(lastFailedCase.TestCase);
                testRunner.KnownResult = lastFailedCase;
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
            var testRunner = new TestRunner(new FullTextTranslator(_translator), _evaluator, settings);
            return testRunner.RunTestSession(testCases);
        }

        public void SavePatterns()
        {
            _evaluator.SavePatterns();
        }

        private IEnumerator<ScoredPattern> EnumerateScoredPatterns(TestCaseResult result)
            => _patternGenerator.GetScoredPatterns(result).GetEnumerator();
    }
}