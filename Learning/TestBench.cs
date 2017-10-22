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
            var testCases = TestRunner.LoadTestCases();
            var results = _testRunner.RunTestCases(testCases);
            _reporter.Report(results);
            return results.All(res => res.Success);
        }

        public TestCaseResult RunTestCase(TestCase testCase)
            => _testRunner.RunTestCase(testCase);
    }
}