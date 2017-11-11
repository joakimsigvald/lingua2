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
            var candidates = Decode(from).Select(t => new[] { t }).ToList();
            var patterns = PatternExtractor.GetMatchingPatterns(candidates, 2);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase("N")]
        [TestCase("NV", "^N*V", "^NV", "^N*V*", "^NV*")]
        [TestCase("ANV", "^A*N", "^AN", "^A*N*", "^AN*", "A*N*V*", "AN*V*", "A*NV*", "A*N*V", "A*NV", "AN*V", "ANV*", "ANV")]
        public void ExtracTripplePatterns(string from, params string[] expected)
        {
            var candidates = Decode(from).Select(t => new [] {t}).ToList();
            var patterns = PatternExtractor.GetMatchingPatterns(candidates, 3);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase("AN", "^A*", "^A", "^N*", "^N")]
        [TestCase("AN,V", "^A*", "^A", "^N*", "^N", "A*V*", "A*V", "AV*", "AV", "N*V*", "N*V", "NV*", "NV")]
        public void ExtracTwinCandidatePatterns(string from, params string[] expected)
        {
            var candidates = from.Split(',').Select(Decode).ToList();
            var patterns = PatternExtractor.GetMatchingPatterns(candidates, 2);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase("I will be running", "jag kommer att springa", "R1XfVf")]
        public void Test(string from, string to, params string[] expectedPatterns)
        {
            var testCaseResult = TestHelper.GetTestCaseResult(from, to);
            var candidates = testCaseResult.ExpectedCandidates;
            var patterns = PatternExtractor.GetMatchingPatterns(candidates, 3);
            Assert.That(patterns.Intersect(expectedPatterns), Is.EqualTo(expectedPatterns));
        }

        private static Translation[] Decode(string pattern)
            => Encoder.Deserialize(pattern).Select(token => new Translation { From = token }).ToArray();
    }
}