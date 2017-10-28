using System.Linq;
using Lingua.Core;
using Lingua.Testing;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class PatternExtractorTests
    {
        [TestCase("N", "N*", "N")]
        [TestCase("Nn", "N*", "Nn")]
        public void ExtractMonoPatterns(string from, params string[] expected)
        {
            var atoms = Encoder.Encode(from);
            var patterns = PatternExtractor.GetMatchingMonoPatterns(atoms);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase("N", "^N*", "^N")]
        [TestCase("NV", "^N*", "^N", "N*V", "NV", "N*V*", "NV*")]
        public void ExtracTwinPatterns(string from, params string[] expected)
        {
            var atoms = Encoder.Encode(from);
            var patterns = PatternExtractor.GetMatchingPatterns(atoms, 2);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase("N")]
        [TestCase("NV", "^N*V", "^NV", "^N*V*", "^NV*")]
        [TestCase("ANV", "^A*N", "^AN", "^A*N*", "^AN*", "A*N*V*", "AN*V*", "A*NV*", "A*N*V", "A*NV", "AN*V", "ANV*", "ANV")]
        public void ExtracTripplePatterns(string from, params string[] expected)
        {
            var atoms = Encoder.Encode(from);
            var patterns = PatternExtractor.GetMatchingPatterns(atoms, 3);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }

        [TestCase("AN", "^A*", "^A", "^N*", "^N")]
        [TestCase("AN,V", "^A*", "^A", "^N*", "^N", "A*V*", "A*V", "AV*", "AV", "N*V*", "N*V", "NV*", "NV")]
        public void ExtracTwinCandidatePatterns(string from, params string[] expected)
        {
            var candidates = from.Split(',').Select(Encoder.Encode).ToList();
            var patterns = PatternExtractor.GetMatchingPatterns(candidates, 2);
            Assert.That(patterns, Is.EquivalentTo(expected));
        }
    }
}
