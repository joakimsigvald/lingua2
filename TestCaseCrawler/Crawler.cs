using HtmlAgilityPack;
using Lingua.Grammar;
using Lingua.Learning;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestCaseCrawler
{
    public class Crawler
    {
        private const int LetterCount = 1;
        private const int WordCountPerLetter = 1;
        private static readonly char[] letters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        private static readonly HttpClient _client = new HttpClient();

        private readonly string _baseUrl;

        public Crawler(string baseUrl) => _baseUrl = baseUrl;

        public async Task Crawl()
        {
            var wordPaths = await ExtractWordPaths();
            var testCases = await Task.WhenAll(wordPaths.Select(ExtractTestCases));
            StoreTestCases(testCases.SelectMany(x => x).ToArray());
        }

        private async Task<string[]> ExtractWordPaths()
        {
            var wordPaths = await Task.WhenAll(letters.Take(LetterCount).Select(ExtractWordPaths));
            return wordPaths.SelectMany(x => x).ToArray();
        }

        private async Task<string[]> ExtractWordPaths(char letter)
        {
            var menuPage = await LoadPage($"{letter}/");
            var wordMenuPaths = GetWordMenuPaths(menuPage, letter).Take(1).ToArray();
            var wordPagePaths = await Task.WhenAll(wordMenuPaths.Select(GetWordPagePaths));
            return wordPagePaths.SelectMany(x => x).Take(WordCountPerLetter).ToArray();
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

        private async Task<TestCase[]> ExtractTestCases(string wordPath)
        {
            var page = await LoadPage(wordPath);
            return ExtractTestCases(page).ToArray();
        }

        private IEnumerable<TestCase> ExtractTestCases(HtmlNode page)
        {
            /*Searching for
<div class="dict-entry">
  <div class="dict-example">
    <div class="dict-source"><span class="flag uk">English</span>Flight 115 to <b>Albuquerque</b>, New Mexico.</div>
    <div class="dict-result"><span class="icon-link-wrapper dropdown"><a id="resultinteract801" data-target="#" href="#" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false" class="icon-link"><i class="material-icons">more_vert</i></a>
      <ul aria-labeled-by="resultinteract801" class="dropdown-menu">
        <li><a href="http://www.opensubtitles.org/" target="_blank" rel="noopener"><i class="material-icons">open_in_new</i> Link to source</a></li>
        <li><a href="javascript:;" class="cs-flag" data-cs-id="csensv:4202665">
          <i class="material-icons">warning</i> Request revision</a></li>
      </ul></span>Flight 115 till Albuquerque, New Mexico.</div>
  </div>
</div>             */

            var dictEntries = page
                .Descendants("div")
                .Where(d => d.HasClass("dict-entry"))
                .ToArray();
            var dictExamples = dictEntries
                .SelectMany(n => n.ChildNodes.Where(dex => dex.Name == "div" && dex.HasClass("dict-example")))
                .ToArray();
            return dictExamples.Select(ExtractTestCase);
        }

        private TestCase ExtractTestCase(HtmlNode exampleDiv)
        {
            /*Extract from for
    <div class="dict-source"><span class="flag uk">English</span>Flight 115 to <b>Albuquerque</b>, New Mexico.</div>
    <div class="dict-result"><span class="icon-link-wrapper dropdown"><a id="resultinteract801" data-target="#" href="#" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false" class="icon-link"><i class="material-icons">more_vert</i></a>
      <ul aria-labeled-by="resultinteract801" class="dropdown-menu">
        <li><a href="http://www.opensubtitles.org/" target="_blank" rel="noopener"><i class="material-icons">open_in_new</i> Link to source</a></li>
        <li><a href="javascript:;" class="cs-flag" data-cs-id="csensv:4202665">
          <i class="material-icons">warning</i> Request revision</a></li>
      </ul></span>Flight 115 till Albuquerque, New Mexico.</div>
             */
            var source = ExtractPhrase(exampleDiv, "dict-source");
            var result = ExtractPhrase(exampleDiv, "dict-result");
            return new TestCase(CleanPhrase(source), CleanPhrase(result));
        }

        private string ExtractPhrase(HtmlNode exampleDiv, string className)
        {
            var sourceDiv = exampleDiv.ChildNodes.Single(n => n.HasClass(className));
            return ExtractPhrase(sourceDiv);
        }

        private string ExtractPhrase(HtmlNode phraseDiv) 
            => string.Join("", phraseDiv.ChildNodes.Skip(1).Select(cn => cn.InnerText));

        private string CleanPhrase(string result)
            => Regex.Replace(result.Trim(), "\\s+", " ");

        private async Task<HtmlNode> LoadPage(string path)
        {
            var html = await _client.GetStringAsync($"{_baseUrl}/{path}");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }

        private void StoreTestCases(TestCase[] testCases)
        {
            var lines = testCases.Select(tc => tc.ToSave()).Prepend("//Sentences").ToArray();
            Repository.StoreTestCases("en-bab-la", lines);
        }
    }
}