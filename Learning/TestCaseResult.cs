using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;

    public class TestCaseResult
    {
        private readonly TranslationResult _translationResult;
        public readonly TranslationTarget TranslationTarget;
        private readonly bool _allowReordered;

        public TestCaseResult(TestCase testCase
            , TranslationResult translationResult
            , TranslationTarget translationTarget
            , bool allowReordered)
        {
            TestCase = testCase;
            _translationResult = translationResult;
            TranslationTarget = translationTarget;
            _allowReordered = allowReordered;
        }

        public TestCase TestCase { get; }
        public string From => TestCase.From;
        public string Expected => TestCase.Expected;
        public string Actual => _translationResult.Translation;
        public bool IsSuccess => _allowReordered && WordTranslationSuccess || Success;
        private bool Success => Actual == TestCase.Expected;
        private bool WordTranslationSuccess => Success 
            || ExpectedWords.Intersect(TranslatedWords).Count() == ExpectedTranslations.Count;
        public IReason Reason => _translationResult.Reason;
        public IList<Translation> ExpectedTranslations => TranslationTarget.Translations;
        public IEnumerable<Translation> Translations => _translationResult.Translations;
        public int ScoreDeficit { get; set; } = -1;

        public override string ToString()
            => $"{From}=>{Expected}/{Actual}:{Success}";

        private IEnumerable<string> ExpectedWords
            => GetWords(ExpectedTranslations);

        private IEnumerable<string> TranslatedWords
            => GetWords(Translations);

        private IEnumerable<string> GetWords(IEnumerable<Translation> translations)
            => translations.Select(t => t.Output.ToLower());
    }
}