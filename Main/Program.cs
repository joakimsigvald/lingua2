using System;
using System.Linq;
using Lingua.Core;
using Lingua.Grammar;
using Lingua.Testing;
using Lingua.Tokenization;
using Lingua.Vocabulary;

namespace Lingua.Main
{
    class Program
    {
        private static readonly ITranslator Translator
            = new Translator(new Tokenizer(), new Thesaurus(), new Engine());

        private static readonly TestBench TestBench
            = new TestBench(Translator);

        static void Main(string[] args)
        {
            if (args.FirstOrDefault() == "Test")
                RunTestSuite();
            else TranslateMAnual();
        }

        private static void TranslateMAnual()
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