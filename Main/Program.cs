using System;
using System.Linq;

namespace Lingua.Main
{
    using Core;
    using Grammar;
    using Testing;
    using Tokenization;
    using Vocabulary;

    class Program
    {
        private static readonly ITokenizer Tokenizer
            = new Tokenizer();

        private static readonly ITranslator Translator
            = new Translator(Tokenizer, new Thesaurus(), new Engine(new Evaluator()));

        private static readonly TestBench TestBench
            = new TestBench(new TestRunner(Translator, Tokenizer), new ConsoleReporter());

        private static readonly Trainer Trainer
            = new Trainer();

        static void Main(string[] args)
        {
            switch (args.FirstOrDefault())
            {
                case "Test":
                    RunTestSuite();
                    break;
                case "Train":
                    RunTrainingSession();
                    break;
                default:
                    TranslateManual();
                    break;
            }
        }

        private static void RunTrainingSession()
        {
            (var failedTestCase, var runTestCount) = Trainer.RunTrainingSession();
            if (failedTestCase != null)
                throw new Exception("Failed on test case " + runTestCount);
            Console.WriteLine($"Successfully completed training session with {runTestCount} cases!");
            Console.ReadKey();
        }

        private static void TranslateManual()
        {
            var original = Input();
            var translation = Translate(original);
            Output(translation);
        }

        private static void RunTestSuite()
        {
            var success = TestBench.RunTestSuites();
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
            => Translator.Translate(text).Translation;

        private static string Input()
        {
            Console.WriteLine("Write in english");
            return Console.ReadLine();
        }
    }
}