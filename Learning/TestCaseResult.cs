using System.Collections.Generic;

namespace Lingua.Learning
{
    using Core;

    public class TestCaseResult
    {
        private readonly TranslationResult _translationResult;
        public readonly TranslationTarget TranslationTarget;

        public TestCaseResult(TestCase testCase
            , TranslationResult translationResult
            , TranslationTarget translationTarget)
        {
            TestCase = testCase;
            _translationResult = translationResult;
            TranslationTarget = translationTarget;
        }

        public TestCase TestCase { get; }
        public string From => TestCase.From;
        public string Expected => TestCase.Expected;
        public string Actual => _translationResult.Translation;
        public bool Success => Actual == TestCase.Expected;
        public IReason Reason => _translationResult.Reason;
        public IList<Translation> ExpectedTranslations => TranslationTarget.Translations;
        public IEnumerable<Translation> Translations => _translationResult.Translations;
        public int ScoreDeficit { get; set; } = -1;

        public override string ToString()
            => $"{From}=>{Expected}/{Actual}:{Success}";
    }
}