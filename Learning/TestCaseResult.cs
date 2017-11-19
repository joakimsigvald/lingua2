using System.Collections.Generic;

namespace Lingua.Learning
{
    using Core;

    public class TestCaseResult
    {
        private readonly TranslationResult _translationResult;

        public TestCaseResult(TestCase testCase
            , TranslationResult translationResult
            , IList<Translation> expectedCandidates)
        {
            TestCase = testCase;
            _translationResult = translationResult;
            ExpectedCandidates = expectedCandidates;
        }

        public TestCase TestCase { get; }
        public string From => TestCase.From;
        public string Expected => TestCase.Expected;
        public string Actual => _translationResult.Translation;
        public bool Success => Actual == TestCase.Expected;
        public IReason Reason => _translationResult.Reason;
        public IList<Translation> ExpectedCandidates { get; }
        public Translation[] Translations => _translationResult.Translations;

        public override string ToString()
            => $"{From}=>{Expected}/{Actual}:{Success}";
    }
}