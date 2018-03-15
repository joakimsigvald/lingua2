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
        [TestCase("Nn", "N*", "Nn*", "Nn")]
        [TestCase("NN", "N*", "N")]
        [TestCase("AAA", "A*", "A")]
        public void ExtractDistinctMonoPatterns(string from, params string[] expected)
        {
            var sequence = Encoder.Encode(from);
            var patterns = PatternExtractor.GetMatchingMonoCodes(sequence)
                .Select(Encoder.Serialize)
                .Distinct();
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase("N", "^N*", "^N")]
        [TestCase("Nd", "^N*", "^Nd*", "^Nd")]
        [TestCase("Ndt", "^N*", "^Nd*", "^Nt*", "^Ndt*", "^Ndt")]
        [TestCase("NV", "^N*", "^N", "N*V", "NV", "N*V*", "NV*")]
        [TestCase("NdV", "^N*", "^Nd*", "^Nd", "N*V", "Nd*V", "NdV", "N*V*", "Nd*V*", "NdV*")]
        public void ExtracTwinPatterns(string from, params string[] expected)
        {
            var sequence = Encoder.Encode(from);
            var patterns = PatternExtractor.GetMatchingCodes(sequence, 2).Select(Encoder.Serialize);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase("N")]
        [TestCase("NV", "^_V*", "^N*V*", "^NV*", "^_V", "^N*V", "^NV")]
        [TestCase("ANV", "^_N*", "A*_V*", "A*_V", "A_V*", "^_N", "A_V", "^A*N", "^AN", "^A*N*", "^AN*", "A*N*V*", "AN*V*", "A*NV*", "A*N*V", "A*NV", "AN*V", "ANV*", "ANV")]
        public void ExtracTripplePatterns(string from, params string[] expected)
        {
            var sequence = Encoder.Encode(from);
            var patterns = PatternExtractor.GetMatchingCodes(sequence, 3).Select(Encoder.Serialize);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase("it", "det", "Rt3")]
        public void TestMonoPatterns(string from, string to, params string[] expectedPatterns)
        {
            var testCaseResult = TestHelper.GetTestCaseResultForAnalysis(from, to);
            var candidates = testCaseResult.ExpectedTranslations;
            var sequence = Encoder.Encode(candidates);
            var patterns = PatternExtractor.GetMatchingMonoCodes(sequence).Select(Encoder.Serialize);
            Assert.That(patterns.Intersect(expectedPatterns), Is.EquivalentTo(expectedPatterns));
        }
    }
}