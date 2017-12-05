using System.Linq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class TestRunnerTests
    {
        private static readonly PatternGenerator PatternGenerator = new PatternGenerator(new TranslationExtractor(), new PatternExtractor());

        [TestCase("I run", "jag springer", "R1V1")]
        [TestCase("The red ball", "Den röda bollen", "TdqAdn")]
        [TestCase("The red ball", "Den röda bollen", "AdnNd")]
        [TestCase("The red ball", "Den röda bollen", "TdqAdnNd")]
        [TestCase("2 [[ball]]", "2 bollar", "QnNn")]
        public void MatchesPositivePatterns(string from, string expected, params string[] expectedPatterns)
        {
            var result = TestHelper.GetTestCaseResultForAnalysis(from, expected);
            var positivePatterns = PatternGenerator.GetScoredPatterns(result)
                .Where(sp => sp.Score > 0)
                .Select(sp => sp.Pattern);
            Assert.That(positivePatterns.Intersect(expectedPatterns), Is.EquivalentTo(expectedPatterns));
        }

        [TestCase("search result", "sökresultat", "N", "N*")]
        public void MatchesNegativePatterns(string from, string expected, params string[] expectedPatterns)
        {
            var result = TestHelper.GetTestCaseResultForAnalysis(from, expected);
            var positivePatterns = PatternGenerator.GetScoredPatterns(result)
                .Where(sp => sp.Score < 0)
                .Select(sp => sp.Pattern);
            Assert.That(positivePatterns.Intersect(expectedPatterns), Is.EquivalentTo(expectedPatterns));
        }
    }
}