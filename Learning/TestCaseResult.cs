using System.Collections.Generic;

namespace Lingua.Testing
{
    using Core;

    public class TestCaseResult
    {
        public TestCaseResult(TestCase testCase
            , TranslationResult translationResult
            , IList<Translation[]> expectedCandidates)
        {
            TestCase = testCase;
            Actual = translationResult.Translation;
            Reason = translationResult.Reason;
            ExpectedCandidates = expectedCandidates;
        }

        public TestCase TestCase { get; }
        public string From => TestCase.From;
        public string Expected => TestCase.Expected;
        public string Actual { get; }
        public bool Success => Actual == TestCase.Expected;
        public IReason Reason { get; }
        public IList<Translation[]> ExpectedCandidates { get; }

        public override string ToString()
            => $"{From}=>{Expected}/{Actual}:{Success}";
    }
}