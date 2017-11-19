using System.Linq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class CandidateFilterTests
    {
        [TestCase("I have been running", "jag har sprungit", "I", "have", "been", "running")]
        [TestCase("I will be running", "jag kommer att springa", "I", "will be", "running")]
        public void Test(string from, string to, params string[] expectedCandidates)
        {
            var translationResult = TestHelper.Translator.Translate(from);
            var toTokens = TestHelper.Tokenizer.Tokenize(to).ToArray();
            var filteredCandidates = CandidateFilter
                .FilterCandidates(translationResult.Possibilities, toTokens)
                .Select(t => t.From.Value);
            Assert.That(filteredCandidates, Is.EquivalentTo(expectedCandidates));
        }
    }
}