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
            IEnumerator<ScoredPattern> evaluatorEnhancements = new List<ScoredPattern>().GetEnumerator();
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
                        evaluatorEnhancements.Dispose();
                        evaluatorEnhancements = EnumerateEvaluatorEnhancements(result.FailedCase);
                    }
                    else evaluatorEnhancements.Reset();
                    bestResult = result;
                }
                var lastFailedCase = result.FailedCase;
                do
                {
                    if (currentScoredPattern != null)
                        _evaluator.Undo(currentScoredPattern);
                    if (!evaluatorEnhancements.MoveNext())
                        return bestResult;
                    currentScoredPattern = evaluatorEnhancements.Current;
                    _evaluator.Do(currentScoredPattern);
                    lastFailedCase = _testRunner.RunTestCase(lastFailedCase.TestCase);
                } while (lastFailedCase.ScoreDeficit >= bestResult.FailedCase.ScoreDeficit);
            }
            evaluatorEnhancements.Dispose();
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

        private IEnumerator<ScoredPattern> EnumerateEvaluatorEnhancements(TestCaseResult result)
            => _patternGenerator.GetEvaluatorEnhancements(result).ScoredPatterns.GetEnumerator();
    }
}