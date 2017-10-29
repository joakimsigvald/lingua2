using System.Linq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class CandidateFilterTests
    {
        [TestCase("I will be running", "jag kommer att springa", "0:I", "1:will be", "3:running")]
        public void Test(string from, string to, params string[] expectedIndexedCandidates)
        {
            var translationResult = TestHelper.Translator.Translate(from);
            var toTokens = TestHelper.Tokenizer.Tokenize(to).ToArray();
            var filteredCandidates = CandidateFilter
                .FilterCandidates(translationResult.Candidates, toTokens)
                .ToList();
            foreach (var expectedIndexedCandidate in expectedIndexedCandidates)
            {
                var pair = expectedIndexedCandidate.Split(':');
                var index = int.Parse(pair[0]);
                var expectedCandidate = pair[1];
                Assert.That(filteredCandidates[index].Any(t => t.From.Value == expectedCandidate), expectedCandidate);
            }
            Assert.Pass();
        }
    }
}
