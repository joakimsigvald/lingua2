using System.Collections.Generic;

namespace Lingua.Learning
{
    using TestCaseTranslators;
    using Core;
    using Grammar;
    using Tokenization;
    using Vocabulary;

    public class Trainer
    {
        private readonly Rearranger _arranger;
        private readonly NewTrainableEvaluator _evaluator;
        private readonly Translator _translator;

        public Trainer()
        {
            _arranger = new Rearranger();
            _evaluator = new NewTrainableEvaluator(_arranger);
            _translator = CreateTranslator();
        }

        public TestSessionResult RunTrainingSession(params TestCase[] testCases)
        {
            var trainingSession = new TrainingSession(_evaluator, _arranger, _translator, testCases);
            var result = trainingSession.LearnPatterns();
            return !result.Success
                ? result
                : VerifyPatterns(testCases);
        }

        public void SavePatterns()
        {
            _evaluator.SavePatterns();
        }

        private Translator CreateTranslator() 
            => new Translator(new Tokenizer(), new Thesaurus(), new NewGrammarEngine(_evaluator), _arranger, new Capitalizer());

        private TestSessionResult VerifyPatterns(IList<TestCase> testCases)
        {
            var settings = new TestRunnerSettings
            {
                AbortOnFail = true,
                AllowReordered = false
            };
            var testRunner = new TestRunner(new FullTextTranslator(_translator), _evaluator, settings);
            return testRunner.RunTestSession(testCases);
        }
    }
}