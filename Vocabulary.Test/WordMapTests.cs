using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using Xunit;

namespace Lingua.Vocabulary.Test
{
    public class WordMapTests
    {
        [Fact]
        public void Empty()
        {
            var wordMap = new WordMap<Unclassified>();
            Assert.Empty(wordMap.Translations);
        }

        [Theory]
        [InlineData("from", "to")]
        [InlineData("hi", "hej")]
        public void OneWord(string from, string to)
        {
            var translation = GetTranslation(from, to);
            Assert.IsAssignableFrom<Word>(translation.From);
            Assert.Equal(from, translation.From.Value);
            Assert.True(translation.IsTranslatedWord);
            Assert.Equal(to, translation.To);
        }

        [Theory]
        [InlineData("hi hop", "hej hopp")]
        public void OneCompoundWord(string from, string to)
        {
            var translation = GetTranslation(from, to);
            Assert.IsAssignableFrom<Word>(translation.From);
            Assert.Equal(from, translation.From.Value);
            Assert.Equal(2, translation.WordCount);
            Assert.False(translation.IsTranslatedWord);
            Assert.True(translation.Matches(GetTokens(from).ToArray(), 1));
            Assert.Equal(to, translation.To);
        }

        [Theory]
        [InlineData("ball:s", "boll:ar")]
        public void OneNoun(string from, string to)
        {
            var translations = GetTranslations<Noun>(new Dictionary<string, string> { { from, to } });
            Assert.Equal(2, translations.Length);
            var stemTranslation = translations[0];
            Assert.Equal(translations, stemTranslation.Variations);
            Assert.IsAssignableFrom<Noun>(stemTranslation.From);
            Assert.Equal(from.Split(':')[0], stemTranslation.From.Value);
            Assert.Equal(to.Split(':')[0], stemTranslation.To);
        }

        [Theory]
        [InlineData("ball:s", "ball", "balls")]
        [InlineData("foot:___eet", "foot", "feet")]
        [InlineData("bouncing ball:s", "bouncing ball", "bouncing balls")]
        public void Variations(string from, params string[] variations)
        {
            var translations = GetTranslations<Noun>(new Dictionary<string, string> { { from, from } });
            Assert.Equal(variations.Length, translations.Length);
            for (var i = 0; i < variations.Length; i++)
                Assert.Equal(variations[i], translations[i].From.Value);
        }

        private ITranslation GetTranslation(string from, string to)
            => GetTranslations<Unclassified>(new Dictionary<string, string> {{from, to}}).Single();

        private static ITranslation[] GetTranslations<TWord>(IDictionary<string, string> mapings)
            where TWord : Word, new()
            => new WordMap<TWord>(mapings).Translations.ToArray();

        private static IEnumerable<Token> GetTokens(string str)
        {
            var parts = str.Split(' ');
            return parts.Select(part => new Unclassified {Value = part});
        }
    }
}