using System.Linq;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using Xunit;

namespace Lingua.Core.Test
{
    public class EncoderTests
    {
        [Theory]
        [InlineData(".", 1 << Encoder.ModifierCount)]
        [InlineData(",", 2 << Encoder.ModifierCount)]
        [InlineData("Q", 3 << Encoder.ModifierCount)]
        [InlineData("N", 4 << Encoder.ModifierCount)]
        [InlineData("Nn", (4 << Encoder.ModifierCount) + 1)]
        [InlineData("Nd", (4 << Encoder.ModifierCount) + 2)]
        [InlineData("Ndn", (4 << Encoder.ModifierCount) + 2 + 1)]
        [InlineData("Ng", (4 << Encoder.ModifierCount) + (1 << 2))]
        [InlineData("Ndg", (4 << Encoder.ModifierCount) + 2 + (1 << 2))]
        [InlineData("Nng", (4 << Encoder.ModifierCount) + 1 + (1 << 2))]
        [InlineData("Ndng", (4 << Encoder.ModifierCount) + 2 + 1 + (1 << 2))]
        [InlineData("NN", 4 << Encoder.ModifierCount, 4 << Encoder.ModifierCount)]
        [InlineData("NdN", (4 << Encoder.ModifierCount) + 2, 4 << Encoder.ModifierCount)]
        [InlineData("T", 5 << Encoder.ModifierCount)]
        [InlineData("Td", (5 << Encoder.ModifierCount) + 2)]
        [InlineData("Tq", (5 << Encoder.ModifierCount) + (1 << 9))]
        [InlineData("Tdq", (5 << Encoder.ModifierCount) + 2 + (1 << 9))]
        [InlineData("TN", 5 << Encoder.ModifierCount, 4 << Encoder.ModifierCount)]
        [InlineData("P", 6 << Encoder.ModifierCount)]
        [InlineData("R", 7 << Encoder.ModifierCount)]
        [InlineData("R1", (7 << Encoder.ModifierCount) + (1 << 4))]
        [InlineData("R1n", (7 << Encoder.ModifierCount) + 1 + (1 << 4))]
        [InlineData("R2", (7 << Encoder.ModifierCount) + (1 << 5))]
        [InlineData("R2n", (7 << Encoder.ModifierCount) + 1 + (1 << 5))]
        [InlineData("R3", (7 << Encoder.ModifierCount) + (1 << 6))]
        [InlineData("R3n", (7 << Encoder.ModifierCount) + 1 + (1 << 6))]
        [InlineData("A", 8 << Encoder.ModifierCount)]
        [InlineData("X", 9 << Encoder.ModifierCount)]
        [InlineData("X1", (9 << Encoder.ModifierCount) + (1 << 4))]
        [InlineData("X2", (9 << Encoder.ModifierCount) + (1 << 5))]
        [InlineData("X3", (9 << Encoder.ModifierCount) + (1 << 6))]
        [InlineData("V", 10 << Encoder.ModifierCount)]
        [InlineData("V1", (10 << Encoder.ModifierCount) + (1 << 4))]
        [InlineData("V3", (10 << Encoder.ModifierCount) + (1 << 6))]
        [InlineData("I", 11 << Encoder.ModifierCount)]
        [InlineData("C", 12 << Encoder.ModifierCount)]
        [InlineData("N*", (4 << Encoder.ModifierCount) + Encoder.Wildcard)]
        [InlineData("Nn*", (4 << Encoder.ModifierCount) + 1 + Encoder.Wildcard)]
        public void EncodeToken(string serial, params int[] expected)
        {
            var tokens = Encoder.Deserialize(serial);
            var actual = Encoder.Encode(tokens).Select(us => (int)us).ToArray();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Modifier.Plural | Modifier.ThirdPerson, "n3")]
        [InlineData(Modifier.Plural | Modifier.SecondPerson, "n2")]
        [InlineData(Modifier.Plural | Modifier.FirstPerson, "n1")]
        public void SerializeModifiers(Modifier modifiers, string expected)
        {
            var token = new Verb {Modifiers = modifiers};
            var actual = Encoder.Serialize(new []{token});
            Assert.Equal($"V{expected}", actual);
        }

        [Theory]
        [InlineData("VpAa")]
        [InlineData("V*")]
        [InlineData("R*")]
        [InlineData("Rt*")]
        [InlineData("Rt")]
        [InlineData("V*.")]
        [InlineData("V,")]
        [InlineData("Nt")]
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
            Assert.Equal(fromTypes, toTypes);
            Assert.Equal(fromModifiers, toModifiers);
            Assert.Equal(fromSymbols, toSymbols);
        }

        [Theory]
        [InlineData("N", "P*", false)]
        [InlineData("P", "N*", false)]
        [InlineData("Rm", "R*", true)]
        [InlineData("V1", "V1*", true)]
        [InlineData("V1t", "V1*", true)]
        [InlineData("V1t", "Vt*", true)]
        [InlineData("R1m", "R*", true)]
        [InlineData("R3tX3R1mN", "R3tX3R*N", true)]
        [InlineData("N", "_", true)]
        public void MatchPatterns(string example, string pattern, bool expected)
        {
            var exampleCode = Encoder.Encode(example);
            var patternCode = Encoder.Encode(pattern);
            var matches = Encoder.Matches(exampleCode, patternCode);
            Assert.Equal(expected, matches);
        }
    }
}