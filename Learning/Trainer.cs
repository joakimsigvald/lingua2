using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;

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
            return !result.Success 
                ? result 
                : VerifyPatterns(testCases);
        }

        private IList<TestCase> PrepareForLearning(IEnumerable<TestCase> testCases)
        {
            var preparedTestCases = testCases
                .Where(tc => !string.IsNullOrWhiteSpace(tc.From))
                .ToList();
            preparedTestCases.ForEach(PrepareForLearning);
            var prioritizedTestCases = preparedTestCases
                .Select(tc => (testcase: tc, priority: ComputePriority(tc.Targets.First().Translations)))
                .OrderBy(pair => pair.priority);
            return prioritizedTestCases
                .Select(pair => pair.testcase)
                .ToList();
        }

        private static int ComputePriority(IReadOnlyCollection<ITranslation> translations)
            => translations.Count
               + translations.Select(t => t.From.GetType()).Distinct().Count()
               + translations.Select(t => t.From)
                   .OfType<Element>()
                   .Select(e => e.Modifiers)
                   .Distinct().Count();

        private void PrepareForLearning(TestCase testCase)
        {
            testCase.Possibilities = _translator.Destruct(testCase.From);
            testCase.Targets = TargetSelector.SelectTargets(testCase.Possibilities, testCase.Expected);
            if (!testCase.Targets.Any())
                throw new Exception("Should not get into this state - throw exception from TargetSelector if no possible translation");
        }

        private TestSessionResult LearnPatterns(IList<TestCase> testCases)
        {
            var settings = new TestRunnerSettings
            {
                AbortOnFail = true,
                AllowReordered = false
            };
            var testRunner = new TestRunner(new PossibitiesTranslator(_translator), _evaluator, settings);

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
                } while (lastFailedCase.Deficit >= bestResult.FailedCase.Deficit);
                if (lastFailedCase.Success)
                    testCases.MoveToBeginning(lastFailedCase.TestCase);
                else if (lastFailedCase.WordTranslationSuccess)
                {
                    throw new Exception("TODO: Learn reordering of " + lastFailedCase);
                }
                testRunner.KnownResult = lastFailedCase;
            }
            return result;
        }
        /*
        private void LearnRearrangements(TestSessionResult result)
        {
            var allTestCases = result.Results
                .Select(tcr => tcr.TestCase)
                .ToArray();
            var outOfOrderCases = allTestCases
                .Where(tc => !tc.Target.Arrangement.IsInPerfectOrder)
                .ToArray();
            if (outOfOrderCases.Any())
                LearnRearrangements(outOfOrderCases, allTestCases);
        }

        private void LearnRearrangements(ICollection<TestCase> outOfOrderCases, IEnumerable<TestCase> allTestCases)
        {
            var arrangerCandidates = GetArrangementCandidates(outOfOrderCases).ToArray();
            LearnRearrangements(allTestCases, outOfOrderCases, arrangerCandidates);
        }

        private static IEnumerable<Arranger> GetArrangementCandidates(IEnumerable<TestCase> outOfOrderCases)
            => ArrangerGenerator.GetArrangerCandidates(GetTargetArrangers(outOfOrderCases));

        private static IEnumerable<Arrangement> GetTargetArrangers(IEnumerable<TestCase> outOfOrderCases)
            => outOfOrderCases
                .Select(tc => tc.Target.Arrangement)
                .Distinct()
                .Where(arr => !arr.IsInPerfectOrder)
                .ToArray();

        private void LearnRearrangements(IEnumerable<TestCase> allTestCases, ICollection<TestCase> outOfOrderCases, IEnumerable<Arranger> arrangerCandidates)
        {
            var inOrderCases = allTestCases.Except(outOfOrderCases).ToArray();
            foreach (var arranger in arrangerCandidates)
            {
                _evaluator.Add(arranger);
                var newInOrderCases = TryNewArranger(inOrderCases, outOfOrderCases);
                if (newInOrderCases.Length == outOfOrderCases.Count)
                    return;
                inOrderCases = inOrderCases.Concat(newInOrderCases).ToArray();
                outOfOrderCases = outOfOrderCases.Except(newInOrderCases).ToArray();
                if (!newInOrderCases.Any())
                    _evaluator.Remove(arranger);
            }
            throw new Exception($"Failed to learn arrangements. Improve algorithm: {string.Join("|", outOfOrderCases)}");
        }

        private TestCase[] TryNewArranger(
            IEnumerable<TestCase> inOrderCases
            , IEnumerable<TestCase> outOfOrderCases)
            => inOrderCases.All(IsCorrectlyArranged)
                ? outOfOrderCases.Where(IsCorrectlyArranged).ToArray()
                : new TestCase[0];

        private bool IsCorrectlyArranged(TestCase testCase)
        {
            var actual = _evaluator.Arrange(testCase.Target.Translations)
                .Where(t => t.From is Element)
                .ToArray();
            var expected = testCase.Target.ArrangedTranslations
                .Where(t => t.From is Element)
                .ToArray();
            var result = actual.SequenceEqual(expected);
            return result;
        }
        */
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
        {
            var patterns = _patternGenerator
                .GetScoredPatterns(result)
                .Select(PrioritizePattern)
                .OrderBy(tuple => tuple.priority)
                .Select(tuple => tuple.sp)
                .ToList();
           return patterns.GetEnumerator();
        }

        private (ScoredPattern sp, int priority) PrioritizePattern(ScoredPattern sp)
            => (sp, ScoredPatternPriorityComputer.ComputePriority(_evaluator.GetScore(sp.Code), sp.Score, sp.Code));
    }
}