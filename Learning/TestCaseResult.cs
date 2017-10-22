using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    using Core;
    using Core.Tokens;

    public class TestCaseResult
    {
        public TestCaseResult(TestCase testCase, TranslationResult translationResult, Token[] toTokens)
        {
            TestCase = testCase;
            Actual = translationResult.Translation;
            Reason = translationResult.Reason;
            ExpectedCandidates = FilterCandidates(translationResult.Candidates, toTokens)?.ToArray();
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

        private static IEnumerable<Translation[]> FilterCandidates(ICollection<Translation[]> candidates,
            IReadOnlyList<Token> toTokens)
            => candidates == null || candidates.Count != toTokens.Count
                ? candidates
                : candidates.Select((c, i) => FilterTranslations(c, toTokens[i]).ToArray());

        private static IEnumerable<Translation> FilterTranslations(
            IEnumerable<Translation> translations,
            Token toToken)
            => translations.Where(t => t.To == toToken.Value);
    }
}