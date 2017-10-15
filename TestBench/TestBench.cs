using System.Linq;

namespace Lingua.Testing
{
    public class TestBench
    {
        private readonly TestRunner _testRunner;
        private readonly IReporter _reporter;

        public TestBench(TestRunner testRunner, IReporter reporter)
        {
            _testRunner = testRunner;
            _reporter = reporter;
        }

        public bool RunTestSuites()
        {
            var results = _testRunner.RunTestCases();
            _reporter.Report(results);
            return results.All(res => res.Success);
        }

        public TestCaseResult RunTestCase(string group, string from, string to)
            => _testRunner.RunTestCase(group, from, to);
    }
}