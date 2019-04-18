using HtmlAgilityPack;
using Lingua.Grammar;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestCaseCrawler
{
    public class PathExtractor
    {
        private const int WordCount = 1;

        private static readonly HttpClient _client = new HttpClient();
        private readonly string _baseUrl;
        private readonly char _letter;

        public PathExtractor(string baseUrl, char letter)
        {
            _baseUrl = baseUrl;
            _letter = letter;
        }

        public async Task IngestPaths()
        {
            var wordPaths = await ExtractWordPaths();
            StoreWordPaths(wordPaths);
        }

        private async Task<string[]> ExtractWordPaths()
        {
            var menuPage = await LoadPage($"{_letter}/");
            var wordMenuPaths = GetWordMenuPaths(menuPage, _letter).Take(1).ToArray();
            var wordPagePaths = await Task.WhenAll(wordMenuPaths.Select(GetWordPagePaths));
            return wordPagePaths.SelectMany(x => x).Take(WordCount).ToArray();
        }

        private IEnumerable<string> GetWordMenuPaths(HtmlNode menuPage, char letter)
        {
            //Search for <li><a href="/dictionary/english-swedish/a/9">Page 9 for letter A </a></li>
            var menuLinkNodes = menuPage
                .Descendants("li")
                .SelectMany(n => n.ChildNodes.Where(cn => cn.Name == "a"))
                .Where(IsMenuLinkNode)
                .ToArray();
            return menuLinkNodes
                .Select((n, i) => $"{letter}/{i + 1}");

            bool IsMenuLinkNode(HtmlNode node)
                => IsMenuLinkText(node.FirstChild.InnerHtml);

            bool IsMenuLinkText(string text)
                => text.StartsWith("Page") && text.Contains(" for letter ");
        }

        private async Task<string[]> GetWordPagePaths(string wordMenuPath)
        {
            const string prefix = "/dictionary/english-swedish/";

            //Search for <li><a href="/dictionary/english-swedish/albuquerque"><span class="flag uk">English</span>  Albuquerque</a></li>
            var menuPage = await LoadPage(wordMenuPath);
            return menuPage
                .Descendants("li")
                .SelectMany(n => n.ChildNodes.Where(cn => cn.Name == "a"))
                .Select(ln => ln.GetAttributeValue("href", ""))
                .Where(IsWordLinkRef)
                .Select(lr => lr.Substring(prefix.Length))
                .ToArray();

            bool IsWordLinkRef(string text)
                => text.StartsWith(prefix) && !text.EndsWith("/") && text != prefix;
        }

        private async Task<HtmlNode> LoadPage(string path)
        {
            var html = await _client.GetStringAsync($"{_baseUrl}/{path}");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }

        private void StoreWordPaths(string[] paths)
        {
            Repository.StoreText($"paths-{_letter}", paths);
        }
    }
}