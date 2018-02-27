using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public TestSessionResult RunTestSession(ICollection<TestCase> testCases)
        {
            //Keep, can be useful with more test cases
            //var results = TestRunnerProcess.Run(_translator, _evaluator, testCases);
            var results = RunTestCases(testCases).ToArray();
            return new TestSessionResult(results);
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

        private static void AssureTargetsSet(TestCase testCase, TranslationResult translationResult)
        {
            if (testCase.Target == null)
                testCase.Targets = TargetSelector.SelectTargets(translationResult.Possibilities, testCase.Expected);
        }
    }

    public class TestRunnerProcess
    {
        private readonly ICollection<TestCase> _testCases;
        private int _failedIndex;
        private readonly ConcurrentBag<(int index, TestCaseResult result)> _results = new ConcurrentBag<(int, TestCaseResult)>();
        private readonly ITestCaseTranslator _translator;
        private readonly TrainableEvaluator _evaluator;
        private Task[] _tasks;

        public static TestCaseResult[] Run(
            ITestCaseTranslator translator
            , TrainableEvaluator evaluator
            , ICollection<TestCase> testCases)
            => new TestRunnerProcess(translator, evaluator, testCases).Run();

        private TestRunnerProcess(
            ITestCaseTranslator translator
            , TrainableEvaluator evaluator
            , ICollection<TestCase> testCases)
        {
            _translator = translator;
            _evaluator = evaluator;
            _testCases = testCases;
            _failedIndex = testCases.Count;
        }

        private TestCaseResult[] Run()
        {
            var firstCase = _testCases.First();
            var firstResult = RunTestCase(firstCase);
            if (!firstResult.Success)
            {
                return new[] {firstResult};
            }
            _results.Add((-1, firstResult));
            _tasks = _testCases
                .Skip(1)
                .Select((tc, i) => Task.Factory.StartNew(() => RunTestTask(i, tc))).ToArray();
            Task.WaitAll(_tasks);
            TestCaseResult lastResult = null;
            var resultsUntilFail = _results
                .OrderBy(indexedRes => indexedRes.index)
                .Select(indexedRes => indexedRes.result)
                .TakeWhile(res => (lastResult?.Success ?? true) && (lastResult = res) == res)
                .ToArray();
            return resultsUntilFail;
        }

        private void RunTestTask(int testIndex, TestCase tc)
        {
            if (testIndex > _failedIndex) return;
            var result = RunTestCase(tc);
            if (!result.Success)
                Fail(testIndex);
            _results.Add((testIndex, result));
        }

        private void Fail(int testIndex)
        {
            lock (this)
            {
                _failedIndex = Math.Min(_failedIndex, testIndex);
            }
        }

        private TestCaseResult RunTestCase(TestCase testCase)
        {
            var translationResult = _translator.Translate(testCase);
            var result = new TestCaseResult(testCase, translationResult);
            if (result.Success != result.SuccessIgnoringCase)
                throw new InvalidExample(testCase, "Expected has invalid casing");
            if (_evaluator != null && !result.Success)
                result.ScoreDeficit = _evaluator.ComputeScoreDeficit(result);
            return result;
        }
    }
}