namespace Lingua.Learning
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
            var result = _testRunner.RunTestSession(testCases);
            _reporter.Report(result);
            return result.Success;
        }

        public TestCaseResult RunTestCase(TestCase testCase)
            => _testRunner.RunTestCase(testCase);
    }
}