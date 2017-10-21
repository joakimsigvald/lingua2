using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    using Core;
    using Core.Tokens;

    public class TestRunner
    {
        private readonly bool _abortOnFail;
        private readonly ITranslator _translator;
        private readonly ITokenizer _tokenizer;

        public TestRunner(ITranslator translator, ITokenizer tokenizer, bool abortOnFail = false)
        {
            _abortOnFail = abortOnFail;
            _translator = translator;
            _tokenizer = tokenizer;
        }

        public TestCaseResult[] RunTestCases()
            => RunTestCases(LoadTestCases()).ToArray();

        private IEnumerable<(string, string, string)> LoadTestCases()
        {
            var testSuites = Loader.LoadTestSuites();
            var testCases = testSuites
                .SelectMany(kvp => kvp.Value.Select(v => (kvp.Key, v.Key, v.Value)));
            return testCases;
        }

        private IEnumerable<TestCaseResult> RunTestCases(IEnumerable<(string, string, string)> testCases)
        {
            TestCaseResult prevResult = null;
            return testCases
                .Select(testCase => RunTestCase(testCase.Item1, testCase.Item2, testCase.Item3))
                .TakeWhile(result =>
                {
                    var abort = !_abortOnFail || (prevResult?.Success ?? true);
                    prevResult = result;
                    return abort;
                });
        }

        public TestCaseResult RunTestCase(string group, string from, string to)
        {
            var toTokens = _tokenizer.Tokenize(to).ToArray();
            var translationResult = _translator.Translate(from);
            return new TestCaseResult
            {
                Group = group,
                From = from,
                Expected = to,
                Actual = translationResult.Translation,
                Reason = translationResult.Reason,
                ExpectedCandidates = FilterCandidates(translationResult.Candidates, toTokens)?.ToArray()
            };
        }

        private static IEnumerable<Translation[]> FilterCandidates(ICollection<Translation[]> candidates, IReadOnlyList<Token> toTokens)
            => candidates == null || candidates.Count != toTokens.Count
                ? candidates
                : candidates.Select((c, i) => FilterTranslations(c, toTokens[i]).ToArray());

        private static IEnumerable<Translation> FilterTranslations(
            IEnumerable<Translation> translations,
            Token toToken)
            => translations.Where(t => t.To == toToken.Value);
    }
}