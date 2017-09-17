using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using NUnit.Framework;

namespace Lingua.Vocabulary.Test
{
    [TestFixture]
    public class ThesaurusTests
    {
        private static readonly IThesaurus Thesaurus = new Thesaurus();

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("Joakim")]
        public void Untranslatable(string from)
            => Assert.That(Thesaurus.Translate(new Unclassified {Value = from}).Single().To, Is.Null);

        [TestCase("ball", "boll")]
        [TestCase("foot", "fot")]
        [TestCase("a", "en")]
        [TestCase("and", "och")]
        [TestCase("my", "min")]
        [TestCase("I", "jag")]
        [TestCase("to", "att")]
        [TestCase("to", "till")]
        [TestCase("play", "leka")]
        [TestCase("with", "med")]
        [TestCase("easy", "lätt")]
        public void Word(string from, string to)
            => Translates(from, to);

        [TestCase("i.e.", "dvs.")]
        [TestCase("e.g.", "t.ex.")]
        public void Abbreviation(string from, string to)
            => Translates(from, to);

        [TestCase("good-looking", "snygg")]
        [TestCase("non-English-speaking", "icke-engelsktalande")]
        [TestCase("non-achievement-oriented", "icke-prestationsorienterad")]
        public void HyphenedDoubleWord(string from, string to)
            => Translates(from, to);

        [TestCase("bouncing ball", "studsboll")]
        [TestCase("easy as pie", "lätt som en pannkaka")]
        [TestCase("easy peasy", "lätt som en plätt")]
        [TestCase("will have been", "kommer att ha")]
        public void CompoundWordOrPhrase(string from, string to)
            => Translates(from, to);

        [TestCase("Non-French-speaking", "Icke-fransktalande")]
        [TestCase("Bouncing ball", "Studsboll")]
        public void Capitalize(string from, string to)
            => Translates(from, to);

        [TestCase("ball", "boll", "bolls", "bollen", "bollens", "bollar", "bollars", "bollarna", "bollarnas")]
        public void Generic(string stem, params string[] expected)
        {
            var candidates = Thesaurus.Translate(new Generic(stem))
                .Select(translation => translation.To)
                .ToArray();
            Assert.That(candidates, Is.EquivalentTo(expected));
        }

        [TestCase("search", "sök")]
        [TestCase("street", "gatu")]
        public void IncompleteCompund(string from, string to)
        {
            var translations = Thesaurus.Translate(new Unclassified {Value = from});
            var incompleteCompound = translations.Single(translation => translation.IsIncompleteCompound);
            Assert.That(incompleteCompound.To, Is.EqualTo(to));
        }

        private static void Translates(string from, string to)
        {
            var word = new Unclassified {Value = from.Split(' ')[0] };
            var translations = Thesaurus.Translate(word)
                .Where(t => t.From.Value == from)
                .Select(t => t.To);
            Assert.That(translations, Does.Contain(to));
        }
    }
}