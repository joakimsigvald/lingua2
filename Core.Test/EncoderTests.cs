using System.Linq;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    [TestFixture]
    public class EncoderTests
    {
        [TestCase(".", 1 << 16)]
        [TestCase(",", 2 << 16)]
        [TestCase("T", 3 << 16)]
        [TestCase("Td", (3 << 16) + 1)]
        [TestCase("Tq", (3 << 16) + (1 << 3))]
        [TestCase("Tdq", (3 << 16) + 1 + (1 << 3))]
        [TestCase("N", 4 << 16)]
        [TestCase("Nd", (4 << 16) + 1)]
        [TestCase("Nn", (4 << 16) + 2)]
        [TestCase("Ndn", (4 << 16) + 1 + 2)]
        [TestCase("Ng", (4 << 16) + (1 << 2))]
        [TestCase("Ndg", (4 << 16) + 1 + (1 << 2))]
        [TestCase("Nng", (4 << 16) + 2 + (1 << 2))]
        [TestCase("Ndng", (4 << 16) + 1 + 2 + (1 << 2))]
        [TestCase("TN", new[] { 3 << 16, 4 << 16 })]
        [TestCase("NN", new[] { 4 << 16, 4 << 16 })]
        [TestCase("NdN", new[] { (4 << 16) + 1, 4 << 16 })]
        [TestCase("R1", (5 << 16) + 16)]
        [TestCase("R1n", (5 << 16) + 2 + (1 << 4))]
        [TestCase("R2", (5 << 16) + (1 << 5))]
        [TestCase("R2n", (5 << 16) + 2 + (1 << 5))]
        [TestCase("R3", (5 << 16) + (3 << 4))]
        [TestCase("R3n", (5 << 16) + 2 + (3 << 4))]
        [TestCase("A", 6 << 16)]
        [TestCase("X", 7 << 16)]
        [TestCase("X1", (7 << 16) + (1 << 4))]
        [TestCase("X2", (7 << 16) + (1 << 5))]
        [TestCase("X3", (7 << 16) + (3 << 4))]
        [TestCase("V", 8 << 16)]
        [TestCase("V1", (8 << 16) + (1 << 4))]
        [TestCase("V3", (8 << 16) + (3 << 4))]
        [TestCase("Q", 9 << 16)]
        public void EncodeToken(string serial, params int[] expected)
        {
            var tokens = Encoder.Deserialize(serial);
            var actual = Encoder.Encode(tokens);
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [TestCase(Modifier.Plural | Modifier.ThirdPerson, "n3")]
        [TestCase(Modifier.Plural | Modifier.SecondPerson, "n2")]
        [TestCase(Modifier.Plural | Modifier.FirstPerson, "n1")]
        public void SerializeModifiers(Modifier modifiers, string expected)
        {
            var token = new Noun {Modifiers = modifiers};
            var actual = Encoder.Serialize(new []{token});
            Assert.That(actual, Is.EquivalentTo($"N{expected}"));
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
            var expected = new [] { (3 << 16) + 1, (4 << 16) + 1 + 4, (4 << 16) + 2 };
            var actual = Encoder.Encode(tokens);
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [TestCase("VpAa")]
        [TestCase("V*")]
        public void EncodeDecode(string fromSymbols)
        {
            var fromTokens = Encoder.Deserialize(fromSymbols).ToArray();
            var fromTypes = fromTokens.Select(t => t.GetType()).ToArray();
            var fromModifiers = fromTokens.Select(t => (t as Element)?.Modifiers).ToArray();
            var code = Encoder.Encode(fromTokens).ToArray();
            var toTokens = Encoder.Decode(code).ToArray();
            var toTypes = toTokens.Select(t => t.GetType()).ToArray();
            var toModifiers = toTokens.Select(t => (t as Element)?.Modifiers).ToArray();
            var toSymbols = Encoder.Serialize(toTokens);
            Assert.That(toTypes, Is.EqualTo(fromTypes));
            Assert.That(toModifiers, Is.EqualTo(fromModifiers));
            Assert.That(toSymbols, Is.EqualTo(fromSymbols));
        }
    }
}