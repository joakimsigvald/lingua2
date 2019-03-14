using System.Linq;
using Xunit;

namespace Lingua.Learning.Test
{
    using Core;

    public class PatternExtractorTests
    {
        private static readonly PatternExtractor PatternExtractor = new PatternExtractor();

        [Theory]
        [InlineData("N", "N*", "N")]
        [InlineData("Nn", "N*", "Nn*", "Nn")]
        [InlineData("NN", "N*", "N")]
        [InlineData("AAA", "A*", "A")]
        public void ExtractDistinctMonoPatterns(string from, params string[] expected)
        {
            var sequence = Encoder.Encode(from);
            var patterns = PatternExtractor.GetMatchingMonoCodes(sequence)
                .Select(Encoder.Serialize)
                .Distinct();
            Assert.Equal(expected, patterns);
        }

        [Theory]
        [InlineData("N", "^N*", "^N")]
        [InlineData("Nd", "^N*", "^Nd*", "^Nd")]
        [InlineData("Ndt", "^N*", "^Nd*", "^Nt*", "^Ndt*", "^Ndt")]
        [InlineData("NV", "^N*", "^N", "N*V", "NV", "N*V*", "NV*")]
        [InlineData("NdV", "^N*", "^Nd*", "^Nd", "N*V", "Nd*V", "NdV", "N*V*", "Nd*V*", "NdV*")]
        public void ExtracTwinPatterns(string from, params string[] expected)
        {
            var sequence = Encoder.Encode(from);
            var patterns = PatternExtractor.GetMatchingCodes(sequence, 2).Select(Encoder.Serialize);
            TestUtils.Assert.Equivalent(expected.ToHashSet(), patterns.ToArray());
        }

        [Theory]
        [InlineData("N")]
        [InlineData("NV", "^_V*", "^N*V*", "^NV*", "^_V", "^N*V", "^NV")]
        [InlineData("ANV", "^_N*", "A*_V*", "A*_V", "A_V*", "^_N", "A_V", "^A*N", "^AN", "^A*N*", "^AN*", "A*N*V*", "AN*V*", "A*NV*", "A*N*V", "A*NV", "AN*V", "ANV*", "ANV")]
        public void ExtracTripplePatterns(string from, params string[] expected)
        {
            var sequence = Encoder.Encode(from);
            var patterns = PatternExtractor.GetMatchingCodes(sequence, 3).Select(Encoder.Serialize);
            TestUtils.Assert.Equivalent(expected.ToHashSet(), patterns.ToArray());
        }

        [Theory]
        [InlineData("it", "det", "Rt3")]
        public void TestMonoPatterns(string from, string to, params string[] expectedPatterns)
        {
            var testCaseResult = TestHelper.GetTestCaseResultForAnalysis(from, to);
            var candidates = testCaseResult.ExpectedTranslations;
            var sequence = Encoder.Encode(candidates);
            var patterns = PatternExtractor.GetMatchingMonoCodes(sequence).Select(Encoder.Serialize);
            Assert.Equal(expectedPatterns, patterns.Intersect(expectedPatterns));
        }
    }
}