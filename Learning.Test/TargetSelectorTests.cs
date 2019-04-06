﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using Xunit;

namespace Lingua.Learning.Test
{
    public class TargetSelectorTests
    {
        [Theory]
        [InlineData("", "", null)]
        [InlineData("x>,y>a", "a", "1")]
        [InlineData("x>a", "a", "0")]
        [InlineData("x>a,y>b", "ab", "01")]
        [InlineData("x>a,y>b", "a", "0")]
        [InlineData("x>a,y>b", "ba", "10")]
        [InlineData("x>a,y>b,z>c", "ca", "20")]
        [InlineData("x>a", "A", "0")]
        [InlineData("x>a/b,y>c", "bc", "01")]
        [InlineData("x>ab,y>abc", "abcab", "10")]
        [InlineData("x>cab/a/ab,y>abc", "abcab", "10")]
        [InlineData("x>a,x>a", "aa", "01")]
        [InlineData("x>a,y>b,x>a", "ab", "01")]
        [InlineData("x>a,y>b,x>a", "ba", "12")]
        [InlineData("x>a,y>bc,z>def", "bcdefa", "120")]
        [InlineData("The>,cat>katten,caught>fångade,a>en,rat>råtta", "Katten fångade en råtta", "1234")]
        [InlineData("The>De/,cat>katten,caught>fångade,a>en,rat>råtta", "Katten fångade en råtta", "1234")]
        [InlineData("A>En,ball>boll,i.e.>dvs.,..>..", "En boll dvs...", "0123")]
        public void TestArrangementOrder(string possibilitiesStr, string to, string expectedOrdersStr)
        {
            var target = CreateTarget(possibilitiesStr, to);
            var expectedOrder = ParseOrder(expectedOrdersStr);
            Assert.Equal(expectedOrder, target?.Arrangement?.Order);
        }

        [Theory]
        [InlineData("x>a", "ab", "b")]
        [InlineData("x>a,y>d", "  a    b   cd  ", "b,c")]
        [InlineData("x>a,x>b", "c d", "c,d")]
        public void TestUnmatched(string possibilitiesStr, string to, string expected)
        {
            var ex = Assert.Throws<Exception>(() => CreateTarget(possibilitiesStr, to));
            Assert.Contains("missing: " + expected, ex.Message);
        }

        [Theory]
        [InlineData("x>a:1", "a", "1")]
        [InlineData("x>a:1,y>b:2", "ba", "12")]
        [InlineData("x>a:1/b:2,y>c:3", "bc", "23")]
        [InlineData("x>a:1/b:2,y>c:3,z>d:4", "db", "234")]
        public void TestArrangementCode(string possibilitiesStr, string to, string expectedCodeStr)
        {
            var target = CreateTarget(possibilitiesStr, to);
            var expectedCode = ParseCode(expectedCodeStr);
            Assert.Equal(expectedCode, target?.Arrangement.Code);
        }

        [Theory]
        [InlineData("x>a", "a", "x>a")]
        [InlineData("x>a/b,y>c", "bc", "x>b,y>c")]
        [InlineData("x>a/b,y>c", "b", "x>b,y>c")]
        [InlineData("x>a:1/b:1,y>c", "b", "x>b,y>c")]
        [InlineData("x>a/a b,y>b", "a b", "x>a b")]
        [InlineData("x>a b/a,y>b", "a b", "x>a b")]
        [InlineData("x>a/,y>b", "b", "x>,y>b")]
        [InlineData("x>a/,y>b,z>a", "ba", "x>,y>b,z>a")]
        [InlineData("The>De/,cat>katten,caught>fångade,a>en,rat>råtta", "Katten fångade en råtta", "The>,cat>katten,caught>fångade,a>en,rat>råtta")]
        public void TestTranslations(string possibilitiesStr, string to, string expectedTranslationsStr)
        {
            var target = CreateTarget(possibilitiesStr, to);
            var expectedTranslations = ParsePossibilities(expectedTranslationsStr)
                .Select(alts => alts.Single()).ToArray();
            var actual = GetOutputs(target?.Translations);
            var expected = GetOutputs(expectedTranslations);
            Assert.Equal(expected, actual);
        }

        private static string[] GetOutputs(IEnumerable<ITranslation> translations)
            => translations.Select(t => t.Output).ToArray();

        private TranslationTarget CreateTarget(string possibilitiesStr, string to)
        {
            var possibilities = ParsePossibilities(possibilitiesStr);
            return TargetSelector.SelectTargets(possibilities, to).FirstOrDefault();
        }

        private IList<ITranslation[]>? ParsePossibilities(string possibilitiesStr)
            => string.IsNullOrEmpty(possibilitiesStr)
                ? null
                : possibilitiesStr.Split(',').Select(ParseAlternatives).ToList();

        private ITranslation[] ParseAlternatives(string alternativesStr)
        {
            var parts = alternativesStr.Split('>');
            var from = parts[0];
            var to = parts[1].Split('/');
            return to.Select((alt, i) => FakeTranslation.Create(from, alt, i)).ToArray();
        }

        private static ushort[]? ParseCode(string codeStr)
            => string.IsNullOrEmpty(codeStr)
                ? null
                : codeStr.Select(c => (ushort)(c - 48)).ToArray();

        private static byte[] ParseOrder(string ordersStr)
            => ordersStr?.Select(c => (byte) (c - 48)).ToArray();
    }

    public class FakeTranslation : ITranslation
    {
        public static ITranslation Create(string from, string to, int index)
        {
            var toParts = to.Split(':');
            var toWord = toParts[0];
            var code = toParts.Length == 1
                ? (ushort)index
                : ushort.Parse(toParts[1]);
            return new FakeTranslation
            {
                Input = from,
                Output = toWord,
                Code = code,
                WordCount = (byte)toWord.Split(' ').Length
        };
        }

        public string Output { get; private set; }
        public Token From => new Unclassified();
        public ushort Code { get; set; }
        public byte WordCount { get; private set; }
        public bool IsCapitalized => false;
        public ITranslation[] Variations { get; set; }
        public bool IsTranslatedWord => false;
        public bool IsIncompleteCompound { get; set; }
        public string? To => null;
        public Word[] Continuation => new Word[0];
        public string Input { get; private set; }
        public ITranslation Capitalize()
        {
            throw new NotImplementedException();
        }

        public bool Matches(IReadOnlyList<Token> tokens, int nextIndex)
        {
            throw new NotImplementedException();
        }

        public ITranslation Decapitalize()
        {
            throw new NotImplementedException();
        }
    }
}