using System.Collections.Generic;

namespace Lingua.Learning
{
    using Core;
    using Grammar;
    using Tokenization;
    using Vocabulary;

    public class Trainer
    {
        private readonly TrainableEvaluator _evaluator;
        private readonly ITranslator _translator;
        private readonly ITokenizer _tokenizer;
        private readonly PatternGenerator _patternGenerator;
        private TestRunner _testRunner;

        public Trainer()
        {
            _evaluator = new TrainableEvaluator();
            _tokenizer = new Tokenizer();
            _translator = new Translator(_tokenizer, new Thesaurus(), new Engine(_evaluator));
            _patternGenerator = new PatternGenerator(new TranslationExtractor(), new PatternExtractor());
        }

        public TestSessionResult RunTrainingSession(params TestCase[] testCases)
        {
            _testRunner = new TestRunner(_translator, _tokenizer, _evaluator, true);
            IEnumerator<(string, sbyte)> scoredPatterns = new List<(string, sbyte)>().GetEnumerator();
            var previousResult = new TestSessionResult();
            (string currentPattern, sbyte currentScore) = (null, 0);
            TestSessionResult result;
            while (!(result = Evaluate(testCases)).Success)
            {
                if (result > previousResult)
                {
                    (currentPattern, currentScore) = (null, 0);
                    scoredPatterns.Dispose();
                    if (result.SuccessCount > previousResult.SuccessCount)
                        scoredPatterns = EnumerateMatchingPatterns(result.FailedCase);
                    else scoredPatterns.Reset();
                    previousResult = result;
                }
                var lastFailedCase = result.FailedCase;
                do
                {
                    _evaluator.UpdateScore(currentPattern, (sbyte)-currentScore);
                    if (!scoredPatterns.MoveNext())
                        return result;
                    (currentPattern, currentScore) = scoredPatterns.Current;
                    _evaluator.UpdateScore(currentPattern, currentScore);
                    lastFailedCase = _testRunner.RunTestCase(lastFailedCase.TestCase);
                } while (lastFailedCase.ScoreDeficit >= result.FailedCase.ScoreDeficit);
            }
            scoredPatterns.Dispose();
            return result;
        }

        private TestSessionResult Evaluate(IEnumerable<TestCase> testCases)
        {
            var result = _testRunner.RunTestCases(testCases);
            if (result.FailedCase != null)
            result.PatternCount = _evaluator.PatternCount;
            return result;
        }

        private IEnumerator<(string, sbyte)> EnumerateMatchingPatterns(TestCaseResult result)
            => _patternGenerator.GetMatchingPatterns(result).GetEnumerator();
    }
}