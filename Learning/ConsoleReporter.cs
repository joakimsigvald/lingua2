using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    public class ConsoleReporter : IReporter
    {
        public void Report(TestSessionResult result)
        {
            var testSuiteResults = GetTestSuiteResults(result).ToList();
            Console.WriteLine(testSuiteResults.All(r => r.Success) ? "Test succeeded!" : "Test failed!");
            Console.WriteLine();
            var failedSuites = testSuiteResults
                .Where(res => !res.Success)
                .ToList();
            if (failedSuites.Any())
                ReportFailed(failedSuites);
            ReportPassed(testSuiteResults.ToList());
        }

        private static IEnumerable<TestSuiteResult> GetTestSuiteResults(TestSessionResult result)
            => result.Results
                .GroupBy(r => r.TestCase.Suite)
                .Select(g => new TestSuiteResult
                {
                    Caption = g.Key,
                    Succeeded = g.Where(tcr => result.Success).ToList(),
                    Failed = g.Where(tcr => !result.Success).ToList()
                });

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
    }
}