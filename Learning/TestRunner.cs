using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;

    public class TestRunner
    {
        private readonly ITranslator _translator;
        private readonly ITokenizer _tokenizer;
        private readonly TrainableEvaluator _evaluator;
        private readonly TestRunnerSettings _settings;

        public TestRunner(ITranslator translator, ITokenizer tokenizer
            , TrainableEvaluator evaluator = null
            , TestRunnerSettings settings = null)
        {
            _settings = settings ?? new TestRunnerSettings();
            _translator = translator;
            _tokenizer = tokenizer;
            _evaluator = evaluator;
        }

        public static TestCase[] LoadTestCases()
            => Loader.LoadTestSuites()
                .SelectMany(kvp => kvp.Value.Select(v => new TestCase(v.Key, v.Value)
                {
                    Suite = kvp.Key
                }))
                .ToArray();

        public TestSessionResult RunTestCases(IEnumerable<TestCase> testCases)
            => new TestSessionResult(RunTestCases(testCases, null).ToArray());

        public TestCaseResult RunTestCase(TestCase testCase)
        {
            var expectedTokens = _tokenizer.Tokenize(testCase.Expected).ToArray();
            var translationResult = _translator.Translate(testCase.From);
            var target = TargetSelector
                .SelectTarget(translationResult.Possibilities, expectedTokens);
            var result = new TestCaseResult(testCase
                , translationResult
                , target
                , _settings.AllowReordered);
            if (_evaluator != null && !result.IsSuccess)
                result.ScoreDeficit = _evaluator.ComputeScoreDeficit(result);
            return result;
        }

        private IEnumerable<TestCaseResult> RunTestCases(
            IEnumerable<TestCase> testCases
            , TestCaseResult prevResult) 
            => testCases
                .Select(RunTestCase)
                .TakeWhile(result =>
                {
                    var again = !_settings.AbortOnFail || (prevResult?.IsSuccess ?? true);
                    prevResult = result;
                    return again;
                });
    }
}