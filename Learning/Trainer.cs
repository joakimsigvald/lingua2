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
            _translator = new Translator(_tokenizer, new Thesaurus(), new GrammarEngine(_evaluator));
            _patternGenerator = new PatternGenerator(new TranslationExtractor(), new PatternExtractor());
        }

        public TestSessionResult RunTrainingSession(params TestCase[] testCases)
        {
            _testRunner = new TestRunner(_translator, _tokenizer, _evaluator, true);
            IEnumerator<ScoredPattern> scoredPatterns = new List<ScoredPattern>().GetEnumerator();
            var bestResult = new TestSessionResult();
            ScoredPattern currentScoredPattern = null;
            TestSessionResult result;
            while (!(result = Evaluate(testCases)).Success)
            {
                if (result > bestResult)
                {
                    currentScoredPattern = null;
                    if (result.SuccessCount > bestResult.SuccessCount)
                    {
                        scoredPatterns.Dispose();
                        scoredPatterns = EnumerateScoredPatterns(result.FailedCase);
                    }
                    else scoredPatterns.Reset();
                    bestResult = result;
                }
                var lastFailedCase = result.FailedCase;
                do
                {
                    if (currentScoredPattern != null)
                        _evaluator.Undo(currentScoredPattern);
                    if (!scoredPatterns.MoveNext())
                        return bestResult;
                    currentScoredPattern = scoredPatterns.Current;
                    _evaluator.Do(currentScoredPattern);
                    lastFailedCase = _testRunner.RunTestCase(lastFailedCase.TestCase);
                } while (lastFailedCase.ScoreDeficit >= bestResult.FailedCase.ScoreDeficit);
            }
            scoredPatterns.Dispose();
            return result;
        }

        public void SavePatterns()
        {
            _evaluator.SavePatterns();
        }

        private TestSessionResult Evaluate(IEnumerable<TestCase> testCases)
        {
            var result = _testRunner.RunTestCases(testCases);
            if (result.FailedCase != null)
                result.PatternCount = _evaluator.PatternCount;
            return result;
        }

        private IEnumerator<ScoredPattern> EnumerateScoredPatterns(TestCaseResult result)
            => _patternGenerator.GetScoredPatterns(result).GetEnumerator();
    }
}