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
            var result = TestHelper.GetTestCaseResult(from, expected);
            var positivePatterns = PatternGenerator.GetMatchingPatterns(result)
                .Where(sp => sp.Item2 > 0)
                .Select(sp => sp.Item1);
            Assert.That(positivePatterns.Intersect(expectedPatterns), Is.EquivalentTo(expectedPatterns));
        }

        [TestCase("search result", "sökresultat", "NN")]
        public void MatchesNegativePatterns(string from, string expected, params string[] expectedPatterns)
        {
            var result = TestHelper.GetTestCaseResult(from, expected);
            var positivePatterns = PatternGenerator.GetMatchingPatterns(result)
                .Where(sp => sp.Item2 < 0)
                .Select(sp => sp.Item1);
            Assert.That(positivePatterns.Intersect(expectedPatterns), Is.EquivalentTo(expectedPatterns));
        }
    }
}