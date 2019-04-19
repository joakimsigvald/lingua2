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
                ? IngestTestCases(crawler, args)
                : IngestPaths(crawler));
            Console.WriteLine("Finished crawling en.bab.la (Press a key to exit)");
            Console.ReadKey();
        }

        private static Task IngestPaths(Crawler crawler)
        {
            Console.WriteLine("Ingest paths");
            return crawler.IngestPaths();
        }

        private static async Task IngestTestCases(Crawler crawler, string[] letters)
        {
            foreach (var letter in letters)
            {
                Console.WriteLine($"Ingest test cases for letter: {letter}");
                await crawler.IngestTestCases(letter[0]);
            }
        }
    }
}