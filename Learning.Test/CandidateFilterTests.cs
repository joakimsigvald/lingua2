using System.Linq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class CandidateFilterTests
    {
        [TestCase("I have been running", "jag har sprungit", "I", "have", "been", "running")]
        [TestCase("I will be running", "jag kommer att springa", "I", "will be", "running")]
        [TestCase("The balls' colour", "Bollarnas färg", "The", "balls'", "colour")]
        [TestCase("Bouncing ball to play with", "Studsboll att leka med", "Bouncing ball", "to", "play", "with")]
        public void Test(string from, string to, params string[] expectedCandidates)
        {
            var translationResult = TestHelper.Translator.Translate(from);
            var toTokens = TestHelper.Tokenizer.Tokenize(to).ToArray();
            var filteredCandidates = TargetSelector
                .SelectTarget(translationResult.Possibilities, toTokens)
                .Translations.Select(t => t.From.Value);
            Assert.That(filteredCandidates, Is.EquivalentTo(expectedCandidates));
        }
    }
}