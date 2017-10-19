using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Testing
{
    public class TestRunner
    {
        private readonly bool _abortOnFail;
        private readonly string[] _suites;
        private readonly ITranslator _translator;
        private readonly int? _caseLimit;

        public TestRunner(ITranslator translator, int? caseLimit = null, bool abortOnFail = false, params string[] suites)
        {
            _abortOnFail = abortOnFail;
            _suites = suites;
            _translator = translator;
            _caseLimit = caseLimit;
        }

        public TestCaseResult[] RunTestCases()
            => RunTestCases(LoadTestCases()).ToArray();

        private IEnumerable<(string, string, string)> LoadTestCases()
        {
            var testSuites = Loader.LoadTestSuites();
            if (_suites.Any())
                testSuites = testSuites.Where(kvp => _suites.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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