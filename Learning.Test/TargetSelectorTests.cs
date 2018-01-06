using System;
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
        [TestCase("x>,y>a", "a", "1")]
        [TestCase("x>a", "a", "0")]
        [TestCase("x>a,y>b", "ab", "01")]
        [TestCase("x>a,y>b", "a", "0")]
        [TestCase("x>a,y>b", "ba", "10")]
        [TestCase("x>a,y>b,z>c", "ca", "20")]
        [TestCase("x>a", "A", "0")]
        [TestCase("x>a/b,y>c", "bc", "01")]
        [TestCase("x>ab,y>abc", "abcab", "10")]
        [TestCase("x>cab/a/ab,y>abc", "abcab", "10")]
        [TestCase("x>a,x>a", "aa", "01")]
        [TestCase("x>a,y>b,x>a", "ab", "01")]
        [TestCase("x>a,y>b,x>a", "ba", "12")]
        [TestCase("x>a,y>bc,z>def", "bcdefa", "120")]
        [TestCase("The>,cat>katten,caught>fångade,a>en,rat>råtta", "Katten fångade en råtta", "1234")]
        [TestCase("The>De/,cat>katten,caught>fångade,a>en,rat>råtta", "Katten fångade en råtta", "1234")]
        [TestCase("A>En,ball>boll,i.e.>dvs.,..>..", "En boll dvs...", "0123")]
        public void TestArrangementOrder(string possibilitiesStr, string to, string expectedOrdersStr)
        {
            var target = CreateTarget(possibilitiesStr, to);
            var expectedOrder = ParseOrder(expectedOrdersStr);
            Assert.That(target?.Arrangement?.Order, Is.EqualTo(expectedOrder));
        }

        [TestCase("x>a", "ab", "b")]
        [TestCase("x>a,y>d", "  a    b   cd  ", "b,c")]
        [TestCase("x>a,x>b", "c d", "c,d")]
        public void TestUnmatched(string possibilitiesStr, string to, string expected)
        {
            var ex = Assert.Throws<Exception>(() => CreateTarget(possibilitiesStr, to));
            Assert.That(ex.Message, Does.Contain("missing: " + expected));
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
        [TestCase("x>a:1/b:1,y>c", "b", "x>b,y>c")]
        [TestCase("x>a/a b,y>b", "a b", "x>a b")]
        [TestCase("x>a b/a,y>b", "a b", "x>a b")]
        [TestCase("x>a/,y>b", "b", "x>,y>b")]
        [TestCase("x>a/,y>b,z>a", "ba", "x>,y>b,z>a")]
        [TestCase("The>De/,cat>katten,caught>fångade,a>en,rat>råtta", "Katten fångade en råtta", "The>,cat>katten,caught>fångade,a>en,rat>råtta")]
        public void TestTranslations(string possibilitiesStr, string to, string expectedTranslationsStr)
        {
            var target = CreateTarget(possibilitiesStr, to);
            var expectedTranslations = ParsePossibilities(expectedTranslationsStr)
                .Select(alts => alts.Single()).ToArray();
            var actual = GetOutputs(target?.Translations);
            var expected = GetOutputs(expectedTranslations);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static string[] GetOutputs(IEnumerable<ITranslation> translations)
            => translations.Select(t => t.Output).ToArray();

        private TranslationTarget CreateTarget(string possibilitiesStr, string to)
        {
            var possibilities = ParsePossibilities(possibilitiesStr);
            return TargetSelector.SelectTargets(possibilities, to).FirstOrDefault();
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
            var toWord = toParts[0];
            var code = toParts.Length == 1
                ? (ushort)index
                : ushort.Parse(toParts[1]);
            return Mock.Of<ITranslation>(translation => translation.Input == from
                                                        && translation.Output == toWord
                                                        && translation.Code == code
                                                        && translation.WordCount == toWord.Split(' ').Length);
        }

        private static ushort[] ParseCode(string codeStr)
            => string.IsNullOrEmpty(codeStr)
                ? null
                : codeStr.Select(c => (ushort)(c - 48)).ToArray();

        private static byte[] ParseOrder(string ordersStr)
            => ordersStr?.Select(c => (byte) (c - 48)).ToArray();
    }
}