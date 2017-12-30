using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Moq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class TargetSelectorTests
    {
        [TestCase("", "", null)]
        [TestCase("x>a", "a", "0")]
        [TestCase("x>a,y>b", "ab", "01")]
        [TestCase("x>a,y>b", "a", "0")]
        [TestCase("x>a,y>b", "ba", "10")]
        [TestCase("x>a,y>b,z>c", "ca", "20")]
        [TestCase("x>A", "a", "0")]
        [TestCase("x>a/b,y>c", "bc", "01")]
        [TestCase("x>a", "ab", "")]
        [TestCase("x>ab,y>abc", "abcab", "10")]
        [TestCase("x>cab/a/ab,y>abc", "abcab", "10")]
        [TestCase("x>a,x>a", "aa", "01")]
        [TestCase("x>a,y>b,x>a", "ab", "01")]
        [TestCase("x>a,y>b,x>a", "ba", "12")]
        public void TestArrangementOrder(string possibilitiesStr, string to, string expectedOrdersStr)
        {
            var target = CreateTarget(possibilitiesStr, to);
            var expectedOrder = ParseOrder(expectedOrdersStr);
            Assert.That(target?.Arrangement?.Order, Is.EqualTo(expectedOrder));
        }

        [TestCase("", "", null)]
        [TestCase("x>a", "a", true)]
        [TestCase("x>a", "ab", false)]
        public void TestIsFullyTranslated(string possibilitiesStr, string to, bool? expected)
        {
            var target = CreateTarget(possibilitiesStr, to);
            Assert.That(target?.IsFullyTranslated, Is.EqualTo(expected));
        }

        [TestCase("x>a", "ab", "b")]
        [TestCase("x>a,y>d", "  a    b   cd  ", "b c")]
        public void TestUnmatched(string possibilitiesStr, string to, string expected)
        {
            var target = CreateTarget(possibilitiesStr, to);
            Assert.That(target.Unmatched, Is.EqualTo(expected));
        }

        [TestCase("x>a:1", "a", "1")]
        [TestCase("x>a:1,y>b:2", "ba", "12")]
        [TestCase("x>a:1/b:2,y>c:3", "bc", "23")]
        [TestCase("x>a:1/b:2,y>c:3,z>d:4", "db", "234")]
        public void TestArrangementCode(string possibilitiesStr, string to, string expectedCodeStr)
        {
            var target = CreateTarget(possibilitiesStr, to);
            var expectedCode = ParseCode(expectedCodeStr);
            Assert.That(target?.Arrangement.Code, Is.EqualTo(expectedCode));
        }

        [TestCase("x>a", "a", "x>a")]
        [TestCase("x>a/b,y>c", "bc", "x>b,y>c")]
        [TestCase("x>a/b,y>c", "b", "x>b,y>c")]
        public void TestTranslations(string possibilitiesStr, string to, string expectedTranslationsStr)
        {
            var target = CreateTarget(possibilitiesStr, to);
            var expectedTranslations = ParsePossibilities(expectedTranslationsStr)
                .Select(alts => alts.Single()).ToArray();
            Assert.That(target?.Translations.Select(t => t.Output), Is.EqualTo(expectedTranslations.Select(t => t.Output)));
        }

        private TranslationTarget CreateTarget(string possibilitiesStr, string to)
        {
            var possibilities = ParsePossibilities(possibilitiesStr);
            return TargetSelector.SelectTarget(possibilities, to);
        }

        private IList<ITranslation[]> ParsePossibilities(string possibilitiesStr)
            => string.IsNullOrEmpty(possibilitiesStr)
                ? null
                : possibilitiesStr.Split(',').Select(ParseAlternatives).ToList();

        private ITranslation[] ParseAlternatives(string alternativesStr)
        {
            var parts = alternativesStr.Split('>');
            var from = parts[0];
            var to = parts[1].Split('/');
            return to.Select((alt, i) => ParseTranslation(from, alt, i)).ToArray();
        }

        private static ITranslation ParseTranslation(string from, string to, int index)
        {
            var toParts = to.Split(':');
            var code = toParts.Length == 1
                ? (ushort)index
                : ushort.Parse(toParts[1]);
            return Mock.Of<ITranslation>(translation => translation.Input == from
                                                        && translation.Output == toParts[0]
                                                        && translation.Code == code);
        }

        private static ushort[] ParseCode(string codeStr)
            => string.IsNullOrEmpty(codeStr)
                ? null
                : codeStr.Select(c => (ushort)(c - 48)).ToArray();

        private static byte[] ParseOrder(string ordersStr)
            => ordersStr?.Select(c => (byte) (c - 48)).ToArray();
    }
}