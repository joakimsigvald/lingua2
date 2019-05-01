using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Tokens;
    using Grammar;
    using TestCaseTranslators;

    internal class TrainingSession
    {
        private readonly IPatternCandidateProvider _patternProvider;
        private readonly Translator _translator;
        private readonly List<TestCase> _testCases;
        private readonly TestRunner _testRunner;
        private TestSessionResult? _bestResult;

        public TrainingSession(ITrainableEvaluator evaluator, Rearranger arranger, Translator translator, IEnumerable<TestCase> testCases)
        {
            _translator = translator;
            _testCases = PrepareForLearning(testCases);
            _testRunner = CreateLearningTestRunner(evaluator);
            _patternProvider = new PatternCandidateProvider(evaluator, _testCases, arranger);
        }

        private TestRunner CreateLearningTestRunner(ITrainableEvaluator evaluator)
        {
            var settings = new TestRunnerSettings
            {
                AbortOnFail = true,
                AllowReordered = false
            };
            return new TestRunner(new PossibitiesTranslator(_translator), evaluator, settings);
        }

        private List<TestCase> PrepareForLearning(IEnumerable<TestCase> testCases)
        {
            var preparedTestCases = testCases
                .Where(tc => !string.IsNullOrWhiteSpace(tc.From))
                .ToList();
            preparedTestCases.ForEach(tc =>  tc.PrepareForLearning(_translator));
            var prioritizedTestCases = preparedTestCases
                .Select(tc => (testcase: tc, priority: ComputePriority(tc.Target.Translations)))
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

        public void LearnPatterns()
        {
            TestSessionResult result;
            while (!(result = RunTestSession()).Success)
                LearnFrom(result);
        }

        public void LearnFrom(TestSessionResult failure)
        {
            if (PreviousSuccessfulCaseFailed(failure))
                _patternProvider.ApplyNextPattern(_bestResult!, failure.FailedCase.TestCase);
            else TryLearnTestCase(failure);
        }

        private TestSessionResult RunTestSession() => _testRunner.RunTestSession(_testCases);

        private bool PreviousSuccessfulCaseFailed(TestSessionResult newResult)
            => newResult.SuccessCount < (_bestResult?.SuccessCount ?? 0);

        private void TryLearnTestCase(TestSessionResult newResult)
        {
            _bestResult = _patternProvider.UpdateFromResult(_bestResult, newResult);
            TryLearnTestCase();
        }

        private void TryLearnTestCase()
        {
            TestCaseResult result;
            do
            {
                _patternProvider.TryNextPattern(_bestResult!);
                result = _testRunner.RunTestCase(_bestResult!.FailedCase.TestCase);
            } while (!result.Success && result <= _bestResult.FailedCase);
        }
    }
}