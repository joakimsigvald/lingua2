using System;
using Lingua.Core;
using Lingua.Grammar;
using Lingua.Tokenization;
using Lingua.Vocabulary;

namespace Lingua.Main
{
    class Program
    {
        private static readonly ITranslator Translator 
            = new Translator(new Tokenizer(), new Thesaurus(), new Engine());

        static void Main(string[] args)
        {
            var original = Input();
            var translation = Translate(original);
            Output(translation);
        }

        private static void Output(string translation)
        {
            Console.WriteLine("Översättning:");
            Console.WriteLine(translation);
            Console.ReadKey();
        }

        private static string Translate(string text)
            => Translator.Translate(text);

        private static string Input()
        {
            Console.WriteLine("Write in english");
            return Console.ReadLine();
        }
    }
}