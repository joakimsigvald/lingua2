using System.Linq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    using Core;

    [TestFixture]
    public class PatternExtractorTests
    {
        private static readonly PatternExtractor PatternExtractor = new PatternExtractor();

        [TestCase("N", "N*", "N")]
        [TestCase("Nn", "N*", "Nn")]
        [TestCase("NN", "N*", "N")]
        [TestCase("AAA", "A*", "A")]
        public void ExtractMonoPatterns(string from, params string[] expected)
        {
            var atoms = Decode(from);
            var patterns = PatternExtractor.GetMatchingMonoPatterns(atoms);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase("N", "^N*", "^N")]
        [TestCase("NV", "^N*", "^N", "N*V", "NV", "N*V*", "NV*")]
        public void ExtracTwinPatterns(string from, params string[] expected)
        {
            var candidates = Decode(from);
            var patterns = PatternExtractor.GetMatchingPatterns(candidates, 2);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase("N")]
        [TestCase("NV", "^N*V", "^NV", "^N*V*", "^NV*")]
        [TestCase("ANV", "^A*N", "^AN", "^A*N*", "^AN*", "A*N*V*", "AN*V*", "A*NV*", "A*N*V", "A*NV", "AN*V", "ANV*", "ANV")]
        public void ExtracTripplePatterns(string from, params string[] expected)
        {
            var candidates = Decode(from);
            var patterns = PatternExtractor.GetMatchingPatterns(candidates, 3);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase(3, "I have been running", "jag har sprungit", "R1X1Xp", "X1XpVlr")]
        [TestCase(3, "I will be running", "jag kommer att springa", "R1XfVf")]
        [TestCase(2, "it is", "det är", "R3tX*")]
        [TestCase(3, "it is my", "det är min", "R3tX*R*")]
        [TestCase(4, "it is my pen", "det är min penna", "R3tX*R*N*")]
        public void TestMultiPatterns(int count, string from, string to, params string[] expectedPatterns)
        {
            var testCaseResult = TestHelper.GetTestCaseResult(from, to);
            var candidates = testCaseResult.ExpectedCandidates;
            var patterns = PatternExtractor.GetMatchingPatterns(candidates, count);
            Assert.That(patterns.Intersect(expectedPatterns), Is.EquivalentTo(expectedPatterns));
        }

        [TestCase("it", "det", "R3t")]
        public void TestMonoPatterns(string from, string to, params string[] expectedPatterns)
        {
            var testCaseResult = TestHelper.GetTestCaseResult(from, to);
            var candidates = testCaseResult.ExpectedCandidates;
            var patterns = PatternExtractor.GetMatchingMonoPatterns(candidates);
            Assert.That(patterns.Intersect(expectedPatterns), Is.EquivalentTo(expectedPatterns));
        }

        private static Translation[] Decode(string pattern)
            => Encoder.Deserialize(pattern).Select(token => new Translation { From = token }).ToArray();
    }
}