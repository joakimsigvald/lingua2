using System;
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

        public int WordDeficit { get; private set; }
        public int ScoreDeficit { get; set; }

        private int ComputeWordDeficit()
        {
            var originalWords = GetWords(Reduction.Translations).ToList();
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
        public bool SuccessIgnoringCase => string.Equals(Actual, TestCase.Expected, StringComparison.InvariantCultureIgnoreCase);
        public ITranslation[] ExpectedTranslations => TestCase.Target.Translations;
        public ReductionResult Reduction => _translationResult.Reduction;

        public override string ToString()
            => $"{TestCase}/{Actual}:{Success}";

        private static IEnumerable<string> GetWords(IEnumerable<ITranslation> translations)
            => translations.Select(t => t.Output.ToLower());

        public void RemoveTarget()
        {
            TestCase.RemoveTarget();
            UpdateDeficit();
        }

        public static bool operator >(TestCaseResult tsr1, TestCaseResult tsr2)
            => tsr1.WordDeficit < tsr2.WordDeficit 
            || tsr1.WordDeficit == tsr2.WordDeficit && tsr1.ScoreDeficit < tsr2.ScoreDeficit;

        public static bool operator <(TestCaseResult tsr1, TestCaseResult tsr2)
            => tsr2 > tsr1;

        private void UpdateDeficit()
        {
            if (TestCase.Target != null)
                WordDeficit = ComputeWordDeficit();
        }
    }
}