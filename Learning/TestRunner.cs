using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Learning
{
    using TestCaseTranslators;

    public class TestRunner
    {
        private readonly ITestCaseTranslator _translator;
        private readonly TrainableEvaluator _evaluator;
        private readonly TestRunnerSettings _settings;

        public TestRunner(ITestCaseTranslator translator
            , TrainableEvaluator evaluator = null
            , TestRunnerSettings settings = null)
        {
            _settings = settings ?? new TestRunnerSettings();
            _translator = translator;
            _evaluator = evaluator;
        }

        public static TestCase[] LoadTestCases()
            => Loader.LoadTestSuites()
                .SelectMany(kvp => kvp.Value.Select(v => new TestCase(v.Key, v.Value)
                {
                    Suite = kvp.Key
                }))
                .ToArray();

        public TestSessionResult RunTestSession(IEnumerable<TestCase> testCases)
            => new TestSessionResult(RunTestCases(testCases).ToArray());

        public TestCaseResult RunTestCase(TestCase testCase)
        {
            var translationResult = _translator.Translate(testCase);
            if (_settings.PrepareTestCaseForAnalysis)
                AssureTargetsSet(testCase, translationResult);
            var result = new TestCaseResult(testCase, translationResult);
            if (result.Success != result.SuccessIgnoringCase)
                throw new InvalidExample(testCase, "Expected has invalid casing");
            if (_evaluator != null && !result.Success)
                result.ScoreDeficit = _evaluator.ComputeScoreDeficit(result);
            return result;
        }

        private IEnumerable<TestCaseResult> RunTestCases(
            IEnumerable<TestCase> testCases)
        {
            TestCaseResult lastResult = null;
            var results = testCases
                .Select(RunTestCase)
                .TakeWhile(result => (lastResult = result).Success)
                .ToList();
            if (lastResult != results.LastOrDefault())
                results.Add(lastResult);
            return results;
        }

        private static void AssureTargetsSet(TestCase testCase, TranslationResult translationResult)
        {
            if (testCase.Target == null)
                testCase.Targets = TargetSelector.SelectTargets(translationResult.Possibilities, testCase.Expected);
        }
    }
}