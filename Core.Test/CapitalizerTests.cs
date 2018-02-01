using System.Linq;
using Lingua.Core.Extensions;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using Moq;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    [TestFixture]
    public class CapitalizerTests
    {
        [TestCase(new[] { "Hi>Hej" }, new[] { 0 }, new[] { "Hej" })]
        [TestCase(new[] { "The>", "cat>katten" }, new[] { 1 }, new[] { "Katten" })]
        [TestCase(new[] { "Are>Är", "you>du", "painting>målar" }, new[] { 2, 1 }, new[] { "Målar", "du" })]
        [TestCase(new[] { "I>jag", "am>är", "painting>målar" }, new[] { 0, 2 }, new[] { "jag", "målar" })]
        [TestCase(new[] { "I>jag", "have>har", "been>varit", "to>till", "the>", "concert hall>konserthallen", ".>." }
            , new[] { 0, 1, 2, 3, 5, 6 }
            , new[] { "Jag", "har", "varit", "till", "konserthallen", "." })]
        [TestCase(new[] { "Today>Idag", "I>jag", "have>har", "been>varit", "to>till", "the>", "concert hall>konserthallen" }
            , new[] { 0, 2, 1, 3, 4, 6 }
            , new[] { "Idag", "har", "jag", "varit", "till", "konserthallen" })]
        [TestCase(new[] { "The>", "rat>råttan", "made>gjorde", "a>ett", "nest>bo", "and>och", "slept>sov", "in>i", "it>det", ".>." }
            , new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }
            , new[] { "Råttan", "gjorde", "ett", "bo", "och", "sov", "i", "det", "." })]
        public void Test(string[] allTranslations, int[] order, string[] expected)
        {
            var all = allTranslations.Select(CreateTranslation).ToArray();
            var arranged = order.Select(i => all[i]).ToArray();
            var capitalizer = new Capitalizer();
            var actual = capitalizer.Capitalize(arranged, all);
            Assert.That(actual.Select(t => t.Output), Is.EquivalentTo(expected));
        }

        private static ITranslation CreateTranslation(string arg)
        {
            var parts = arg.Split('>');
            return CreateTranslation(parts[0], parts[1]);
        }

        private static ITranslation CreateTranslation(string input, string output)
        {
            var translationMock = new Mock<ITranslation>();
            var isCapitalized = IsCapitalized(input, output);
            translationMock.Setup(tran => tran.IsCapitalized).Returns(isCapitalized);
            translationMock.Setup(tran => tran.Input).Returns(input);
            translationMock.Setup(tran => tran.Output).Returns(output);
            translationMock.Setup(tran => tran.Capitalize())
                .Returns(() => CreateTranslation(input.Capitalize(), output.Capitalize()));
            translationMock.Setup(tran => tran.Decapitalize())
                .Returns(() => CreateTranslation(input.Decapitalize(), output.Decapitalize()));
            translationMock.Setup(tran => tran.From)
                .Returns(CreateToken(input));
            return translationMock.Object;
        }

        private static Token CreateToken(string input)
        {
            switch (input)
            {
                case ".": return new Terminator('.');
                default: return new Noun {Value = input};
            }
        }

        private static bool IsCapitalized(string input, string output)
            => input.IsCapitalized() && (output.IsCapitalized() || string.IsNullOrEmpty(output));
    }
}