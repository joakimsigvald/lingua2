using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;

    public class TestCaseResult
    {
        private readonly TranslationResult _translationResult;

        public TestCaseResult(TestCase testCase
            , TranslationResult translationResult)
        {
            TestCase = testCase;
            _translationResult = translationResult;
            UpdateDeficit();
        }

        public int Deficit => WordDeficit + ScoreDeficit;
        private int WordDeficit { get; set; }
        public int ScoreDeficit { private get; set; }

        private int ComputeWordDeficit()
        {
            var originalWords = GetWords(Translations).ToList();
            var translatedWords = originalWords.ToList();
            var expectedWords = GetWords(ExpectedTranslations).ToArray();
            var missingWords = expectedWords.Where(word => !translatedWords.Remove(word)).ToArray();
            var superfluousWords = translatedWords.Except(expectedWords).Distinct().ToArray();
            return missingWords.Length + superfluousWords.Length;
        }

        public TestCase TestCase { get; }
        public string From => TestCase.From;
        public string Expected => TestCase.Expected;
        public string Actual => _translationResult.Translation;
        public bool Success => Actual == TestCase.Expected;
        public IReason Reason => _translationResult.Reason;
        public IEnumerable<ITranslation> ExpectedTranslations => TestCase.Target.Translations;
        public IEnumerable<ITranslation> Translations => _translationResult.Translations;

        public override string ToString()
            => $"{TestCase}/{Actual}:{Success}";

        private static IEnumerable<string> GetWords(IEnumerable<ITranslation> translations)
            => translations.Select(t => t.Output.ToLower());

        public void RemoveTarget()
        {
            TestCase.RemoveTarget();
            UpdateDeficit();
        }

        private void UpdateDeficit()
        {
            if (TestCase.Target != null)
                WordDeficit = ComputeWordDeficit();
        }
    }
}