using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;

    public class TestCaseResult
    {
        private readonly TranslationResult _translationResult;
        private readonly bool _allowReordered;

        public TestCaseResult(TestCase testCase
            , TranslationResult translationResult
            , bool allowReordered)
        {
            TestCase = testCase;
            _translationResult = translationResult;
            _allowReordered = allowReordered;
            WordTranslationSuccess = allowReordered && ComputeWordTranslationSuccess();
        }

        private bool ComputeWordTranslationSuccess()
        {
            var translatedWords = GetWords(Translations).ToList();
            return GetWords(ExpectedTranslations).All(translatedWords.Remove);
        }

        public TestCase TestCase { get; }
        public string From => TestCase.From;
        public string Expected => TestCase.Expected;
        public string Actual => _translationResult.Translation;
        public bool IsSuccess => _allowReordered && WordTranslationSuccess || Success;
        private bool Success => Actual == TestCase.Expected;
        private bool WordTranslationSuccess { get; }
        public IReason Reason => _translationResult.Reason;
        public IList<Translation> ExpectedTranslations => TestCase.Target.Translations;
        public IEnumerable<Translation> Translations => _translationResult.Translations;
        public int ScoreDeficit { get; set; } = -1;

        public override string ToString()
            => $"{From}=>{Expected}/{Actual}:{Success}";

        private IEnumerable<string> GetWords(IEnumerable<Translation> translations)
            => translations.Select(t => t.Output.ToLower());
    }
}