using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;

    public class TestCaseResult : ITestCaseResult
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
            //var originalWords = GetWords(ActualTranslations).ToList();
            //var translatedWords = originalWords.ToList();
            var arrangedWords = _translationResult.Arrangement
                .Select(g => GetWords(g.Translations))
                .ToList();
            var expectedWords = GetWords(ExpectedTranslations);
            var missingWords = expectedWords.Where(word => !RemoveMatch(arrangedWords, word)).ToArray();
            //var superfluousWords = translatedWords.Except(expectedWords).Distinct().ToArray();
            return missingWords.Length + arrangedWords.Count;
        }

        private bool RemoveMatch(List<string[]> arrangedWords, string word)
        {
            var index = arrangedWords.FindIndex(g => g.Contains(word));
            if (index < 0) return false;
            arrangedWords.RemoveAt(index);
            return true;
        }

        public TestCase TestCase { get; }
        public string From => TestCase.From;
        public string Expected => TestCase.Expected;
        public string Actual => _translationResult.Translation;
        public bool Success => Actual == TestCase.Expected;
        public bool SuccessIgnoringCase => string.Equals(Actual, TestCase.Expected, StringComparison.InvariantCultureIgnoreCase);
        public ITranslation[] ExpectedTranslations => TestCase.Target.Translations;
        public IGrammaton[] ExpectedGrammatons => TestCase.Target.Grammatons;
        public ITranslation[] ActualTranslations => _translationResult.Translations;
        public IGrammaton[] ActualGrammatons => _translationResult.Grammatons;
        public int Score => _translationResult.Score;

        public override string ToString()
            => $"{TestCase}/{Actual}:{Success}";

        private static string[] GetWords(IEnumerable<ITranslation> translations)
            => translations.Select(GetWord).ToArray();

        private static string GetWord(ITranslation translation)
            => translation.Output.ToLower();

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