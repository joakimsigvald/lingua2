using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Testing
{
    public class TestBench
    {
        private readonly bool _abortOnFail;
        private readonly ITranslator _translator;
        private readonly IReporter _reporter;
        private readonly int? _caseLimit;

        public TestBench(ITranslator translator, IReporter reporter, int? caseLimit = null, bool abortOnFail = false)
        {
            _abortOnFail = abortOnFail;
            _translator = translator;
            _reporter = reporter;
            _caseLimit = caseLimit;
        }

        public bool RunTestSuites()
        {
            var testCases = LoadTestCases();
            var testCaseResults = RunTestCases(testCases).ToArray();
            _reporter.Report(testCaseResults);
            return testCaseResults.All(res => res.Success);
        }

        private IEnumerable<(string, string, string)> LoadTestCases()
        {
            var testSuites = Loader.LoadTestSuites();
            var testCases = testSuites
                .SelectMany(kvp => kvp.Value.Select(v => (kvp.Key, v.Key, v.Value)));
            if (_caseLimit.HasValue)
                testCases = testCases.Take(_caseLimit.Value);
            return testCases;
        }

        private IEnumerable<TestCaseResult> RunTestCases(IEnumerable<(string, string, string)> testCases)
        {
            TestCaseResult prevResult = null;
            return testCases
                .Select(testCase => RunTestCase(testCase.Item1, testCase.Item2, testCase.Item3))
                .TakeWhile(result =>
                {
                    var abort = !_abortOnFail || (prevResult?.Success ?? true);
                    prevResult = result;
                    return abort;
                });
        }

        public TestCaseResult RunTestCase(string group, string from, string to)
        {
            var translationResult = _translator.Translate(from);
            return new TestCaseResult
            {
                Group = group,
                From = from,
                Expected = to,
                Actual = translationResult.translation,
                Reason = translationResult.reason,
                Success = translationResult.translation == to
            };
        }
    }
}