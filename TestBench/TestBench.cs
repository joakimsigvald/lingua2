using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Testing
{
    public class TestBench
    {
        private readonly ITranslator _translator;

        public TestBench(ITranslator translator) => _translator = translator;

        public bool RunTestSuites()
        {
            var testSuites = Loader.LoadTestSuites();
            var testSuiteResults = new List<TestSuiteResult>();
            foreach (var testSuite in testSuites)
            {
                var testSuiteResult = new TestSuiteResult { Caption = testSuite.Key };
                var results = RunTestSuite(testSuite.Value).ToList();
                testSuiteResult.Succeeded = results.Where(result => result.Success).ToList();
                testSuiteResult.Failed = results.Where(result => !result.Success).ToList();
                testSuiteResults.Add(testSuiteResult);
            }
            var success = testSuiteResults.All(res => res.Success);
            Report(testSuiteResults, success);
            return success;
        }

        public TestCaseResult RunTestCase(string from, string to)
        {
            var translationResult = _translator.Translate(@from);
            return new TestCaseResult
            {
                From = @from,
                Expected = to,
                Actual = translationResult.translation,
                Reason = translationResult.reason,
                Success = translationResult.translation == to
            };
        }

        private static void Report(List<TestSuiteResult> testSuiteResults, bool success)
        {
            Console.WriteLine(success ? "Test succeeded!" : "Test failed!");
            Console.WriteLine();
            var failedSuites = testSuiteResults
                .Where(res => !res.Success)
                .ToList();
            if (failedSuites.Any())
                ReportFailed(failedSuites);
            ReportPassed(testSuiteResults.ToList());
        }

        private static void ReportFailed(List<TestSuiteResult> failedSuites)
        {
            Console.WriteLine("Failed");
            Console.WriteLine("======");
            failedSuites.ForEach(ReportFailed);
        }

        private static void ReportPassed(List<TestSuiteResult> testSuiteResults)
        {
            Console.WriteLine("Passed");
            Console.WriteLine("======");
            testSuiteResults.ForEach(ReportSucceeded);
        }

        private static void ReportSucceeded(TestSuiteResult res)
        {
            Console.WriteLine(res.Caption);
            Console.WriteLine(new string('-', res.Caption.Length));
            foreach (var tcr in res.Succeeded)
                Console.WriteLine($"|{tcr.From}| => |{tcr.Actual}|");
            Console.WriteLine();
        }

        private static void ReportFailed(TestSuiteResult res)
        {
            Console.WriteLine(res.Caption);
            Console.WriteLine(new string('-', res.Caption.Length));
            foreach (var tcr in res.Failed)
                Console.WriteLine($"|{tcr.From}| /=> |{tcr.Expected}| \\\\ |{tcr.Actual}|");
            Console.WriteLine();
        }

        private IEnumerable<TestCaseResult> RunTestSuite(Dictionary<string, string> testCases)
            => testCases.Select(testCase => RunTestCase(testCase.Key, testCase.Value));
    }
}