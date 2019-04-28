using System.Linq;
using Lingua.Core.Extensions;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using Moq;
using Xunit;

namespace Lingua.Core.Test
{
    public class CapitalizerTests
    {
        [Theory]
        [InlineData(new string[0], new string[0])]
        [InlineData(new[] { "hej" }, new[] { "hi>hej" })]
        [InlineData(new[] { "Hej" }, new[] { "Hi>Hej" })]
        [InlineData(new[] { "Målar", "du" }, new[] { "Are>Är", "you>du", "painting>målar" }, 2, 1)]
        [InlineData(new[] { "direktören" }, new[] { "the>", "Director>Direktören" }, 1)]
        [InlineData(new[] { "biträdande", "direktören" },
            new[] { "the>", "Assisting>Biträdande", "Director>Direktören" }, 1, 2)]
        [InlineData(new[] { "jag", "heter", "Joakim" }, new[] { "I>jag", "am>heter", "Joakim" })]
        [InlineData(new[] { "Jag", "heter", "Joakim", "." }, new[] { "I>jag", "am>heter", "Joakim", ".:." })]
        [InlineData(new[] { "En", "fågel", ".", "En", "bil", "." }, new[] { "A>En", "bird>fågel", ".:.", "A>En", "car>bil", ".:." })]
        [InlineData(new[] { "En", "fågel", ".", "En", "bil" }, new[] { "A>En", "bird>fågel", ".:.", "A>En", "car>bil" })]
        [InlineData(new[] { "En", "fågel", ".", "en", "bil", "." }, new[] { "A>En", "bird>fågel", ".:.", "a>en", "car>bil", ".:." })]
        [InlineData(new[] { "Målar", "du", "?", "Målar", "han", "?" }, new[] { "Are>Är", "you>du", "painting>målar", ".:?", "Is>Är", "he>han", "painting>målar", ".:?" }, 2, 1, 3, 6, 5, 7)]
        [InlineData(new[] { "Katten" }, new[] { "The>", "cat>katten" }, 1)]
        [InlineData(new[] { "Albrechts", "Guld" }, new[] { "U:Albrecht's>Albrechts", "N:Gold>Guld" })]
        [InlineData(new[] { "Jag", "har", "varit", "till", "konserthallen", "." }, 
            new[] { "I>jag", "have>har", "been>varit", "to>till", "the>", "concert hall>konserthallen", ".:." },
            0, 1, 2, 3, 5, 6)]
        [InlineData(new[] { "Idag", "har", "jag", "varit", "till", "konserthallen" },
            new[] { "Today>Idag", "I>jag", "have>har", "been>varit", "to>till", "the>", "concert hall>konserthallen" },
            0, 2, 1, 3, 4, 6)]
        [InlineData(new[] { "Råttan", "gjorde", "ett", "bo", "och", "sov", "i", "det", "." },
            new[] { "The>", "rat>råttan", "made>gjorde", "a>ett", "nest>bo", "and>och", "slept>sov", "in>i", "it>det", ".:." },
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9)]
        public void Test(string[] expected, string[] allTranslations, params int[] order)
        {
            var translations = allTranslations
                .Select(CreateTranslation)
                .ToArray();
            var original = translations
                .Select(t => new Grammaton(t))
                .ToArray();
            var arranged = order.Any() ? order.Select(i => translations[i]).ToArray() : translations;
            var capitalizer = new Capitalizer();
            var actual = capitalizer.Capitalize(arranged, original).Select(t => t.Output).ToArray();
            Assert.Equal(expected, actual);
        }

        private static ITranslation CreateTranslation(string arg)
        {
            var parts = arg.Split(':');
            return parts.Length == 1
                ? CreateTranslation(arg, null)
                : CreateTranslation(parts[1], arg[0]);
        }

        private static ITranslation CreateTranslation(string mapping, char? type)
        {
            var parts = mapping.Split('>');
            return CreateTranslation(parts.First(), parts.Last(), 
                type ?? (parts.Length == 1 ? 'U' : 'N'));
        }

        private static ITranslation CreateTranslation(string input, string output, char type)
        {
            var translationMock = new Mock<ITranslation>();
            var isCapitalized = IsCapitalized(input, output);
            translationMock.Setup(tran => tran.IsCapitalized).Returns(isCapitalized);
            translationMock.Setup(tran => tran.Input).Returns(input);
            translationMock.Setup(tran => tran.Output).Returns(output);
            translationMock.Setup(tran => tran.Capitalize())
                .Returns(() => CreateTranslation(input.Capitalize(), output.Capitalize(), type));
            translationMock.Setup(tran => tran.Decapitalize())
                .Returns(() => CreateTranslation(input.Decapitalize(), output.Decapitalize(), type));
            translationMock.Setup(tran => tran.From)
                .Returns(CreateToken(input, type));
            return translationMock.Object;
        }

        private static Token CreateToken(string input, char type)
            => type switch {
                '.' => (Token)new Terminator(input[0]),
                'N' => new Noun { Value = input },
                _ => new Unclassified { Value = input }
            };

        private static bool IsCapitalized(string input, string output)
            => input.IsCapitalized() && (output.IsCapitalized() || string.IsNullOrEmpty(output));
    }
}