using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using NUnit.Framework;

namespace Lingua.Vocabulary.Test
{
    [TestFixture]
    public class WordMapTests
    {
        [Test]
        public void Empty()
        {
            var wordMap = new WordMap<Unclassified>();
            Assert.That(wordMap.Translations, Is.Empty);
        }

        [TestCase("from", "to")]
        [TestCase("hi", "hej")]
        public void OneWord(string from, string to)
        {
            var translation = GetTranslation(from, to);
            Assert.That(translation.From, Is.InstanceOf<Word>());
            Assert.That(translation.From.Value, Is.EqualTo(from));
            Assert.That(translation.IsTranslatedWord, Is.True);
            Assert.That(translation.To, Is.EqualTo(to));
        }

        [TestCase("hi hop", "hej hopp")]
        public void OneCompoundWord(string from, string to)
        {
            var translation = GetTranslation(from, to);
            Assert.That(translation.From, Is.InstanceOf<Word>());
            Assert.That(translation.From.Value, Is.EqualTo(from));
            Assert.That(translation.WordCount, Is.EqualTo(2));
            Assert.That(translation.IsTranslatedWord, Is.False);
            Assert.That(translation.Matches(GetTokens(from).ToArray(), 1));
            Assert.That(translation.To, Is.EqualTo(to));
        }

        [TestCase("ball:s", "boll:ar")]
        public void OneNoun(string from, string to)
        {
            var translations = GetTranslations<Noun>(new Dictionary<string, string> { { from, to } });
            Assert.That(translations.Count, Is.EqualTo(2));
            var stemTranslation = translations[0];
            Assert.That(stemTranslation.Variations, Is.EquivalentTo(translations));
            Assert.That(stemTranslation.From, Is.InstanceOf<Noun>());
            Assert.That(stemTranslation.From.Value, Is.EqualTo(from.Split(':')[0]));
            Assert.That(stemTranslation.To, Is.EqualTo(to.Split(':')[0]));
        }

        [TestCase("ball:s", "ball", "balls")]
        [TestCase("foot:___eet", "foot", "feet")]
        [TestCase("bouncing ball:s", "bouncing ball", "bouncing balls")]
        public void Variations(string from, params string[] variations)
        {
            var translations = GetTranslations<Noun>(new Dictionary<string, string> { { from, from } });
            Assert.That(translations.Count, Is.EqualTo(variations.Length));
            for (var i = 0; i < variations.Length; i++)
                Assert.That(translations[i].From.Value, Is.EqualTo(variations[i]));
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