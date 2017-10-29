using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;

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

        public static TestCase[] LoadTestCases()
            => Loader.LoadTestSuites()
                .SelectMany(kvp => kvp.Value.Select(v => new TestCase(v.Key, v.Value)
                {
                    Suite = kvp.Key
                }))
                .ToArray();

        public TestCaseResult[] RunTestCases(IEnumerable<TestCase> testCases)
        {
            TestCaseResult prevResult = null;
            return testCases
                .Select(RunTestCase)
                .TakeWhile(result =>
                {
                    var abort = !_abortOnFail || (prevResult?.Success ?? true);
                    prevResult = result;
                    return abort;
                })
                .ToArray();
        }

        public TestCaseResult RunTestCase(TestCase testCase)
        {
            var expectedTokens = _tokenizer.Tokenize(testCase.Expected).ToArray();
            var translationResult = _translator.Translate(testCase.From);
            var expectedCandidates = CandidateFilter
                .FilterCandidates(translationResult.Candidates, expectedTokens)
                ?.ToArray();
            return new TestCaseResult(testCase
                , _translator.Translate(testCase.From)
                , expectedCandidates);
        }
    }
}