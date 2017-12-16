using System.Linq;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    [TestFixture]
    public class EncoderTests
    {
        [TestCase(".", 1 << Encoder.ModifierCount)]
        [TestCase(",", 2 << Encoder.ModifierCount)]
        [TestCase("Q", 3 << Encoder.ModifierCount)]
        [TestCase("N", 4 << Encoder.ModifierCount)]
        [TestCase("Nn", (4 << Encoder.ModifierCount) + 1)]
        [TestCase("Nd", (4 << Encoder.ModifierCount) + 2)]
        [TestCase("Ndn", (4 << Encoder.ModifierCount) + 2 + 1)]
        [TestCase("Ng", (4 << Encoder.ModifierCount) + (1 << 2))]
        [TestCase("Ndg", (4 << Encoder.ModifierCount) + 2 + (1 << 2))]
        [TestCase("Nng", (4 << Encoder.ModifierCount) + 1 + (1 << 2))]
        [TestCase("Ndng", (4 << Encoder.ModifierCount) + 2 + 1 + (1 << 2))]
        [TestCase("NN", new[] { 4 << Encoder.ModifierCount, 4 << Encoder.ModifierCount })]
        [TestCase("NdN", new[] { (4 << Encoder.ModifierCount) + 2, 4 << Encoder.ModifierCount })]
        [TestCase("T", 5 << Encoder.ModifierCount)]
        [TestCase("Td", (5 << Encoder.ModifierCount) + 2)]
        [TestCase("Tq", (5 << Encoder.ModifierCount) + (1 << 9))]
        [TestCase("Tdq", (5 << Encoder.ModifierCount) + 2 + (1 << 9))]
        [TestCase("TN", new[] { 5 << Encoder.ModifierCount, 4 << Encoder.ModifierCount })]
        [TestCase("P", 6 << Encoder.ModifierCount)]
        [TestCase("R", 7 << Encoder.ModifierCount)]
        [TestCase("R1", (7 << Encoder.ModifierCount) + (1 << 4))]
        [TestCase("R1n", (7 << Encoder.ModifierCount) + 1 + (1 << 4))]
        [TestCase("R2", (7 << Encoder.ModifierCount) + (1 << 5))]
        [TestCase("R2n", (7 << Encoder.ModifierCount) + 1 + (1 << 5))]
        [TestCase("R3", (7 << Encoder.ModifierCount) + (1 << 6))]
        [TestCase("R3n", (7 << Encoder.ModifierCount) + 1 + (1 << 6))]
        [TestCase("A", 8 << Encoder.ModifierCount)]
        [TestCase("X", 9 << Encoder.ModifierCount)]
        [TestCase("X1", (9 << Encoder.ModifierCount) + (1 << 4))]
        [TestCase("X2", (9 << Encoder.ModifierCount) + (1 << 5))]
        [TestCase("X3", (9 << Encoder.ModifierCount) + (1 << 6))]
        [TestCase("V", 10 << Encoder.ModifierCount)]
        [TestCase("V1", (10 << Encoder.ModifierCount) + (1 << 4))]
        [TestCase("V3", (10 << Encoder.ModifierCount) + (1 << 6))]
        [TestCase("I", 11 << Encoder.ModifierCount)]
        [TestCase("C", 12 << Encoder.ModifierCount)]
        [TestCase("N*", (4 << Encoder.ModifierCount) + Encoder.Wildcard)]
        [TestCase("Nn*", (4 << Encoder.ModifierCount) + 1 + Encoder.Wildcard)]
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
            var token = new Verb {Modifiers = modifiers};
            var actual = Encoder.Serialize(new []{token});
            Assert.That(actual, Is.EquivalentTo($"V{expected}"));
        }

        [TestCase("VpAa")]
        [TestCase("V*")]
        [TestCase("R*")]
        [TestCase("Rt*")]
        [TestCase("Rt")]
        [TestCase("V*.")]
        [TestCase("V,")]
        [TestCase("Nt")]
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

        [TestCase("N", "P*", false)]
        [TestCase("P", "N*", false)]
        [TestCase("Rm", "R*", true)]
        [TestCase("V1", "V1*", true)]
        [TestCase("V1t", "V1*", true)]
        [TestCase("V1t", "Vt*", true)]
        [TestCase("R1m", "R*", true)]
        [TestCase("R3tX3R1mN", "R3tX3R*N", true)]
        public void MatchPatterns(string example, string pattern, bool expected)
        {
            var exampleCode = Encoder.Encode(example);
            var patternCode = Encoder.Encode(pattern);
            var matches = Encoder.Matches(exampleCode, patternCode);
            Assert.That(matches, Is.EqualTo(expected));
        }
    }
}