using System;
using System.Linq;

namespace Lingua.Main
{
    using Learning.TestCaseTranslators;
    using Core;
    using Grammar;
    using Learning;
    using Tokenization;
    using Vocabulary;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(string.Join(", ", args));
            switch (args.FirstOrDefault())
            {
                case "Test":
                    RunTestSuite();
                    break;
                case "Learn":
                    RunTrainingSession();
                    break;
                default:
                    TranslateManual();
                    break;
            }
        }

        private static void RunTrainingSession()
        {
            var testCases = TestRunner.LoadTestCases();
            var trainer = new Trainer();
            var result = trainer.RunTrainingSession(testCases);
            if (!result.Success)
                throw new Exception("Fail!!");
        }

        private static void TranslateManual()
        {
            var original = Input();
            var translation = Translate(original);
            Output(translation);
        }

        private static void RunTestSuite()
        {
            var translator = new FullTextTranslator(CreateTranslator());
            var testBench =
                //new TestBench(new TestRunner(translator), new ConsoleReporter());
            new TestBench(new TestRunner(translator), new NoReporter());
            var success = testBench.RunTestSuites();
            if (!success)
                throw new Exception("Fail!!");
        }

        private static void Output(string translation)
        {
            Console.WriteLine("Översättning:");
            Console.WriteLine(translation);
            Console.ReadKey();
        }

        private static string Translate(string text)
        {
            var translator = CreateTranslator();
            return translator.Translate(text).Translation;
        }

        private static string Input()
        {
            Console.WriteLine("Write in english");
            return Console.ReadLine();
        }

        private static ITranslator CreateTranslator()
        {
            var evaluator = new Evaluator();
            evaluator.Load();
            return new Translator(new Tokenizer(), new Thesaurus(), new GrammarEngine(evaluator));
        }
    }

    public class NoReporter : IReporter
    {
        public void Report(TestSessionResult result)
        {
            //nope
        }
    }
}