using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using Lingua.Grammar;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    [TestFixture]
    public class EncoderTests
    {
        [TestCase(".", 1 << 8)]
        [TestCase(",", 2 << 8)]
        [TestCase("T", 3 << 8)]
        [TestCase("Td", (3 << 8) + 1)]
        [TestCase("Tq", (3 << 8) + 8)]
        [TestCase("Tdq", (3 << 8) + 1 + 8)]
        [TestCase("N", 4 << 8)]
        [TestCase("Nd", (4 << 8) + 1)]
        [TestCase("Nn", (4 << 8) + 2)]
        [TestCase("Ndn", (4 << 8) + 1 + 2)]
        [TestCase("Ng", (4 << 8) + 4)]
        [TestCase("Ndg", (4 << 8) + 1 + 4)]
        [TestCase("Nng", (4 << 8) + 2 + 4)]
        [TestCase("Ndng", (4 << 8) + 1 + 2 + 4)]
        [TestCase("TN", new[] { 3 << 8, 4 << 8 })]
        [TestCase("NN", new[] { 4 << 8, 4 << 8 })]
        [TestCase("NdN", new[] { (4 << 8) + 1, 4 << 8 })]
        [TestCase("R1", (5 << 8) + 16)]
        [TestCase("R1n", (5 << 8) + 2 + 16)]
        [TestCase("R2", (5 << 8) + 32)]
        [TestCase("R2n", (5 << 8) + 2 + 32)]
        [TestCase("R3", (5 << 8) + 48)]
        [TestCase("R3n", (5 << 8) + 2 + 48)]
        [TestCase("A", 6 << 8)]
        [TestCase("V", 7 << 8)]
        [TestCase("V1", (7 << 8) + 16)]
        [TestCase("V3", (7 << 8) + 48)]
        [TestCase("X", 8 << 8)]
        [TestCase("X1", (8 << 8) + 16)]
        [TestCase("X2", (8 << 8) + 32)]
        [TestCase("X3", (8 << 8) + 48)]
        [TestCase("Q", 9 << 8)]
        public void SerializeToken(string serial, params int[] expected)
        {
            var tokens = Encoder.Deserialize(serial);
            var actual = Encoder.Code(tokens);
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void SkipDividersWhenSerializeTokens()
        {
            var tokens = new Token[]
            {
                new Article {Modifiers = Modifier.Definite}, // The
                new Divider(),
                new Noun {Modifiers = Modifier.Definite | Modifier.Genitive}, // child's
                new Divider(),
                new Noun {Modifiers = Modifier.Plural} // toys
            };
            var expected = new [] { (3 << 8) + 1, (4 << 8) + 1 + 4, (4 << 8) + 2 };
            var actual = Encoder.Code(tokens);
            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }
}