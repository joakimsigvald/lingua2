using Lingua.Core;
using Moq;
using System.Linq;
using Xunit;

namespace Lingua.Learning.Test
{
    public class OrderMakerTests
    {
        [Theory]
        [InlineData(new byte[] { 0, 1, 3, 7, 6, 8, 9, 10, 11, 12, 13, 14 },
            "Vad gäller delegationen är representanten utsedd, liksom den biträdande representanten.",
            "Vad", "gäller", "den", "delegationen", ",", "den", "representanten", "är", "utsedd", ",", "liksom", "den", "biträdande", "representanten", ".")]
        [InlineData(new byte[] { 1, 0, 3 },
            "är representanten representanten",
            "representanten", "är", "den", "representanten")]
        public void Test(byte[] expectedOrder, string translation, params string[] words)
        {
            var target = new OrderMaker(translation);
            var translations = words.Select(CreateTranslation).ToArray();
            var actual = target.SelectAndOrderTranslations(translations);
            Assert.Equal(expectedOrder, actual.order);
        }

        private ITranslation CreateTranslation(string output) {
            var mock = new Mock<ITranslation>();
            mock.Setup(t => t.Output).Returns(output);
            return mock.Object;
        }
    }
}