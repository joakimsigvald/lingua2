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
            WordTranslationSuccess = allowReordered && Deficit == 0;
        }

        public int Deficit
        {
            get
            {
                var originalWords = GetWords(Translations).ToList();
                var translatedWords = originalWords.ToList();
                var expectedWords = GetWords(ExpectedTranslations).ToArray();
                var missingWords = expectedWords.Where(word => !translatedWords.Remove(word)).ToArray();
                var superfluousWords = translatedWords.Except(expectedWords).Distinct().ToArray();
                return missingWords.Length + superfluousWords.Length + ScoreDeficit;
            }
        }

        public int ScoreDeficit { private get; set; }

        public TestCase TestCase { get; }
        public string From => TestCase.From;
        public string Expected => TestCase.Expected;
        public string Actual => _translationResult.Translation;
        public bool IsSuccess => _allowReordered && WordTranslationSuccess || Success;
        public bool Success => Actual == TestCase.Expected;
        private bool WordTranslationSuccess { get; }
        public IReason Reason => _translationResult.Reason;
        public IEnumerable<ITranslation> ExpectedTranslations => TestCase.Target.Translations;
        public IEnumerable<ITranslation> Translations => _translationResult.Translations;

        public override string ToString()
            => $"{TestCase}/{Actual}:{Success}";

        private static IEnumerable<string> GetWords(IEnumerable<ITranslation> translations)
            => translations.Select(t => t.Output.ToLower());
    }
}