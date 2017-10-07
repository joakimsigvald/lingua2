using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lingua.Grammar;
using Lingua.Tokenization;
using Lingua.Vocabulary;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    [TestFixture]
    public class TranslatorTests
    {
        private static readonly ITranslator Translator
            = new Translator(new Tokenizer(), new Thesaurus(), new Engine());

        [Test]
        public void TranslateNull()
            => TestCase(null, "");
             
        [TestCase("|It is my pen| /=> |Det är min penna|")]
        [TestCase("|I am painting the wall| /=> |jag målar väggen|")]
        public void RunTestCase(string testCase)
        {
            var parts = Regex.Split(testCase, @"\s+/=>\s+");
            var from = parts[0].Trim('|');
            var to = parts[1].Trim('|');
            TestCase(from, to);
        }

        [Test]
        public void RunTestSuites()
        {
            var testSuites = Learning.Loader.LoadTestSuites();
            var testSuiteResults = new List<TestSuiteResult>();
            foreach (var testSuite in testSuites)
            {
                var testSuiteResult = new TestSuiteResult { Caption = testSuite.Key};
                var results = RunTestSuite(testSuite.Value).ToList();
                testSuiteResult.Succeeded = results.Where(result => result.Success).ToList();
                testSuiteResult.Failed = results.Where(result => !result.Success).ToList();
                testSuiteResults.Add(testSuiteResult);
            }
            var success = testSuiteResults.All(res => res.Success);
            Report(testSuiteResults, success);
            Assert.That(success);
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

        private static IEnumerable<TestCaseResult> RunTestSuite(Dictionary<string, string> testCases)
            => testCases.Select(testCase => RunTestCase(testCase.Key, testCase.Value));

        private static void TestCase(string from, string to)
        {
            var result = RunTestCase(from, to);
            if (!result.Success)
                Output(result.Reason);
            Assert.That(result.Success);
        }

        private static void Output(IReason reason)
            => reason.Evaluations.ForEach(Output);

        private static void Output(IEvaluation evaluation)
            => Console.WriteLine($"{evaluation.Fragment}:{evaluation.Symbols}:{evaluation.Score}");

        private static TestCaseResult RunTestCase(string from, string to)
        {
            var translationResult = Translator.Translate(from);
            return new TestCaseResult
            {
                From = from,
                Expected = to,
                Actual = translationResult.translation,
                Reason = translationResult.reason,
                Success = translationResult.translation == to
            };
        }
    }
}