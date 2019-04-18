using Lingua.Grammar;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestCaseCrawler
{
    public class Crawler
    {
        private const int WordBatchSize = 10;

        private static readonly char[] letters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        private readonly string _baseUrl;

        public Crawler(string baseUrl) => _baseUrl = baseUrl;

        public async Task IngestPaths()
        {
            foreach (var letter in letters)
            {
                var extractor = new PathExtractor(_baseUrl, letter);
                await extractor.IngestPaths();
                await Task.Delay(1000);
            }
        }

        public async Task IngestTestCases(char letter)
        {
            var paths = LoadPaths(letter);
            var batches = GetBatches(paths, WordBatchSize);
            var extractors = batches.Select((b, i) => new TestCaseExtractor(_baseUrl, $"{letter}-{i + 1}", b));
            foreach (var extractor in extractors)
            {
                await extractor.IngestTestCases();
                await Task.Delay(1000);
            }
        }

        private IEnumerable<string[]> GetBatches(string[] paths, int batchSize)
            => Enumerable.Range(0, (paths.Length + batchSize - 1) / batchSize)
            .Select(i => paths.Skip(i * batchSize).Take(batchSize).ToArray());

        private string[] LoadPaths(char letter)
        {
            var fileName = $"paths-{letter}.txt";
            return Repository.Load(fileName);
        }
    }
}