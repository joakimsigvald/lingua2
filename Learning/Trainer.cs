using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core.Extensions;
    using TestCaseTranslators;
    using Core;
    using Grammar;
    using Tokenization;
    using Vocabulary;

    public class Trainer
    {
        private readonly TrainableEvaluator _evaluator;
        private readonly Translator _translator;
        private readonly IGrammar _grammar;
        private readonly PatternGenerator _patternGenerator;

        public Trainer()
        {
            _evaluator = new TrainableEvaluator();
            _grammar = new GrammarEngine(_evaluator);
            _translator = new Translator(new Tokenizer(), new Thesaurus(), _grammar);
            _patternGenerator = new PatternGenerator(new TranslationExtractor(), new PatternExtractor());
        }

        public TestSessionResult RunTrainingSession(params TestCase[] testCases)
        {
            var preparedTestCases = PrepareForLearning(testCases);
            var result = LearnPatterns(preparedTestCases);
            if (!result.Success)
                return result;
            LearnRearrangements(result);
            return VerifyPatterns(testCases);
        }

        private IList<TestCase> PrepareForLearning(IEnumerable<TestCase> testCases)
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

        private void LearnRearrangements(TestSessionResult result)
        {
            var outOfOrderCases = result.Results
                .Where(tcr => !tcr.TestCase.Target.Arrangement.IsInPerfectOrder)
                .Select(tcr => tcr.TestCase)
                .ToArray();
            if (outOfOrderCases.Any())
                LearnRearrangements(outOfOrderCases);
        }

        private void LearnRearrangements(ICollection<TestCase> outOfOrderCases)
        {
            var arrangerCandidates = GetArrangementCandidates(outOfOrderCases);
            LearnRearrangements(outOfOrderCases, arrangerCandidates);
        }

        private static IEnumerable<Arranger> GetArrangementCandidates(IEnumerable<TestCase> outOfOrderCases) 
            => ArrangerGenerator
            .GetArrangerCandidates(GetTargetArrangers(outOfOrderCases))
            .Select(arr => new Arranger(arr));

        private static IEnumerable<Arrangement> GetTargetArrangers(IEnumerable<TestCase> outOfOrderCases) 
            => outOfOrderCases
            .Select(tc => tc.Target.Arrangement)
            .Distinct()
            .Where(arr => !arr.IsInPerfectOrder)
            .ToArray();

        private void LearnRearrangements(ICollection<TestCase> testCases, IEnumerable<Arranger> arrangerCandidates)
        {
            var bestResult = 0;
            foreach (var arranger in arrangerCandidates)
            {
                _evaluator.Add(arranger);
                var result = testCases.Count(IsCorrectlyArranged);
                if (result <= bestResult)
                    _evaluator.Remove(arranger);
                else if (result == testCases.Count)
                    return;
                bestResult = result;
            }
            throw new Exception("Failed to learn arrangements. Improve algorithm");
        }

        private bool IsCorrectlyArranged(TestCase testCase)
        {
            var actual = _evaluator.Arrange(testCase.Target.Translations);
            var expected = testCase.Target.ArrangedTranslations;
            return actual.SequenceEqual(expected);
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
            => _patternGenerator
            .GetScoredPatterns(result)
            .Select(sp => (pattern: sp, priority: ComputePriority(sp)))
            .OrderBy(tuple => tuple.priority)
            .Select(tuple => tuple.pattern)
            .ToList()
            .GetEnumerator();

        private int ComputePriority(ScoredPattern sp)
            => sp.Size * Math.Abs(_evaluator.GetScore(sp.Code));
    }
}