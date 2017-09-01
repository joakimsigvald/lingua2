using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using NUnit.Framework;

namespace Lingua.Grammar.Test
{
    [TestFixture]
    public class EncoderTests
    {
        [TestCase(".", 1 << 8)]
        [TestCase(",", 2 << 8)]
        [TestCase("T", 3 << 8)]
        [TestCase("Td", (3 << 8) + 1)]
        [TestCase("N", 4 << 8)]
        [TestCase("Nd", (4 << 8) + 1)]
        [TestCase("Nn", (4 << 8) + 2)]
        [TestCase("Ndn", (4 << 8) + 1 + 2)]
        [TestCase("Np", (4 << 8) + 4)]
        [TestCase("Ndp", (4 << 8) + 1 + 4)]
        [TestCase("Nnp", (4 << 8) + 2 + 4)]
        [TestCase("Ndnp", (4 << 8) + 1 + 2 + 4)]
        [TestCase("TN", new[] { 3 << 8, 4 << 8 })]
        [TestCase("NN", new[] { 4 << 8, 4 << 8 })]
        [TestCase("NdN", new[] { (4 << 8) + 1, 4 << 8 })]
        [TestCase("Q", 5 << 8)]
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
                new DefiniteArticle(), // The
                new Divider(),
                new Noun {Modifiers = Modifier.Definite | Modifier.Possessive}, // child's
                new Divider(),
                new Noun {Modifiers = Modifier.Plural} // toys
            };
            var expected = new [] { (3 << 8) + 1, (4 << 8) + 1 + 4, (4 << 8) + 2 };
            var actual = Encoder.Code(tokens);
            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }
}