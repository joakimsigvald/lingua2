using System.Collections.Generic;

namespace Lingua.Learning
{
    using TestCaseTranslators;
    using Core;
    using Grammar;
    using Tokenization;
    using Vocabulary;
    using Lingua.Translation;

    public class Trainer
    {
        private readonly Rearranger _arranger;
        private readonly Evaluator _evaluator;
        private readonly TrainableEvaluator _trainableEvaluator;
        private readonly Translator _translator;

        public Trainer()
        {
            _arranger = new Rearranger();
            _evaluator = Evaluator.Create();
            _trainableEvaluator = new TrainableEvaluator(_arranger, _evaluator);
            _translator = CreateTranslator();
        }

        public TestSessionResult RunTrainingSession(params TestCase[] testCases)
        {
            var trainingSession = new TrainingSession(_trainableEvaluator, _arranger, _translator, testCases);
            trainingSession.LearnPatterns();
            return VerifyPatterns(testCases);
        }

        public void SavePatterns()
        {
            _trainableEvaluator.SavePatterns();
        }

        private Translator CreateTranslator() 
            => new Translator(new TokenGenerator(new Tokenizer()), new Thesaurus(), new GrammarEngine(_evaluator), _arranger, new SynonymResolver(), new Capitalizer());

        private TestSessionResult VerifyPatterns(IList<TestCase> testCases)
        {
            var settings = new TestRunnerSettings
            {
                AbortOnFail = true,
                AllowReordered = false
            };
            var testRunner = new TestRunner(new FullTextTranslator(_translator), _trainableEvaluator, settings);
            return testRunner.RunTestSession(testCases);
        }
    }
}