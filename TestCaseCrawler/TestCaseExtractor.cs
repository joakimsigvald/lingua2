using HtmlAgilityPack;
using Lingua.Grammar;
using Lingua.Learning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestCaseCrawler
{
    public class TestCaseExtractor
    {
        private static readonly HttpClient _client = new HttpClient();

        private readonly string _baseUrl;
        private readonly string _tag;
        private readonly string[] _paths;

        public TestCaseExtractor(string baseUrl, string tag, string[] paths)
        {
            _baseUrl = baseUrl;
            _tag = tag;
            _paths = paths;
        }

        public async Task IngestTestCases()
        {
            var testCases = await Task.WhenAll(_paths.Select(ExtractTestCases));
            StoreTestCases(testCases.SelectMany(x => x).ToArray());
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
            Repository.StoreText($"testcases-{_tag}", lines);
        }
    }
}