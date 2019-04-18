using System;
using System.Threading.Tasks;

namespace TestCaseCrawler
{
    class Program
    {
        const string BaseUrl = "https://en.bab.la/dictionary/english-swedish/";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Start crawling en.bab.la");
            await new Crawler(BaseUrl).Crawl();
            Console.WriteLine("Finished crawling en.bab.la (Press a key to exit)");
            Console.ReadKey();
        }
    }
}