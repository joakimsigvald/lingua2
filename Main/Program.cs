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
        private static readonly ITranslator Translator
            = new Translator(new Tokenizer(), new Thesaurus(), new Engine(new Evaluator()));

        private static readonly TestBench TestBench
            = new TestBench(new TestRunner(Translator), new ConsoleReporter());

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
                    RunTrainingSession(10);
                    break;
                default:
                    TranslateManual();
                    break;
            }
        }

        private static void RunTrainingSession(int exCount)
        {
            var success = Trainer.RunTrainingSession(exCount);
            if (!success)
                throw new Exception("Fail!!");
            Console.WriteLine($"Successfully completed training session with {exCount} cases!");
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
            => Translator.Translate(text).translation;

        private static string Input()
        {
            Console.WriteLine("Write in english");
            return Console.ReadLine();
        }
    }
}