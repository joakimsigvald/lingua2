using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Lingua.Vocabulary.Test
{
    using Core.Extensions;
    using Core.WordClasses;

    [TestFixture]
    public class LexiconTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("Anything")]
        public void Empty(string key)
        {
            var lexicon = CreateLexicon();
            var translation = lexicon.Lookup(key);
            Assert.That(translation, Is.Empty);
        }

        [TestCase("a", "b")]
        [TestCase("hi", "hej")]
        public void Word(string from, string to)
            => CanLookup(from, to);

        [Test]
        public void Words()
        {
            var lower = new[] {"x", "y", "z"};
            var words = lower.ToDictionary(l => l, l => l.ToUpper());
            var lexicon = CreateLexicon(words);
            words.ForEach(w => CanLookup(lexicon, w.Key, w.Value));
        }

        [Test]
        public void Dictionaries()
        {
            var first = new[] { "a", "b", "c" }.ToDictionary(l => l, l => l.ToUpper());
            var second = new[] { "x", "y", "z" }.ToDictionary(l => l, l => l.ToUpper());
            var lexicon = CreateLexicon(first, second);
            first.ForEach(w => CanLookup(lexicon, w.Key, w.Value));
            second.ForEach(w => CanLookup(lexicon, w.Key, w.Value));
        }

        [TestCase("ball:s", "boll:ar", new[] { "ball", "balls" }, new[] { "boll", "bollar" })]
        [TestCase("foot:___eet", "fot:__ötter", new[] { "foot", "feet" }, new[] { "fot", "fötter" })]
        [TestCase("choice:s", "val:", new[] { "choice", "choices" }, new[] { "val", "val" })]
        public void ExpandedWords(string from, string to, string[] expandedFrom, string[] expandedTo)
        {
            var lexicon = CreateLexicon(new Dictionary<string, string> { { from, to } });
            var stemTranslation = lexicon.Lookup(expandedFrom[0]).Single();
            var variants = stemTranslation.Variations.Select(variation => variation.To).ToArray();
            Assert.That(variants, Is.EquivalentTo(expandedTo));
            for (var i = 0; i < expandedFrom.Length; i++)
                CanLookup(lexicon, expandedFrom[i], expandedTo[i]);
        }

        private static void CanLookup(string from, string to)
        {
            var lexicon = CreateLexicon(new Dictionary<string, string> {{from, to}});
            CanLookup(lexicon, from, to);
        }

        private static Lexicon CreateLexicon(params IDictionary<string, string>[] maps) 
            => new Lexicon(new List<IModificationRule>(), maps.Select(map => new WordMap<Unclassified>(map)).Cast<IWordMap>().ToArray());

        private static void CanLookup(ILexicon lexicon, string from, string to)
        {
            var translation = lexicon.Lookup(from).Single();
            Assert.That(translation.To, Is.EqualTo(to));
        }
    }
}