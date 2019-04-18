using System;
using System.Linq;
using System.Threading.Tasks;

namespace TestCaseCrawler
{
    class Program
    {
        const string BaseUrl = "https://en.bab.la/dictionary/english-swedish/";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Start crawling en.bab.la");
            var crawler = new Crawler(BaseUrl);
            await (args.Any()
                ? IngestTestCases(crawler, args[0])
                : IngestPaths(crawler));
            Console.WriteLine("Finished crawling en.bab.la (Press a key to exit)");
            Console.ReadKey();
        }

        private static Task IngestPaths(Crawler crawler)
        {
            Console.WriteLine("Ingest paths");
            return crawler.IngestPaths();
        }

        private static Task IngestTestCases(Crawler crawler, string letter)
        {
            Console.WriteLine($"Ingest test cases for letter: {letter}");
            return crawler.IngestTestCases(letter[0]);
        }
    }
}