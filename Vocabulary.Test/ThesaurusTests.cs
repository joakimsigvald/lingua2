using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using Xunit;

namespace Lingua.Vocabulary.Test
{
    public class ThesaurusTests
    {
        private static readonly IThesaurus Thesaurus = new Thesaurus();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("Joakim")]
        [InlineData(".")]
        [InlineData(",")]
        [InlineData(":")]
        public void Untranslatable(string from)
            => Assert.Null(Thesaurus.Translate(new Unclassified { Value = from }).Single().To);

        [Theory]
        [InlineData("ball", "boll")]
        [InlineData("foot", "fot")]
        [InlineData("a", "en")]
        [InlineData("and", "och")]
        [InlineData("my", "min")]
        [InlineData("I", "jag")]
        [InlineData("to", "att")]
        [InlineData("to", "till")]
        [InlineData("play", "leka")]
        [InlineData("with", "med")]
        [InlineData("easy", "lätt")]
        public void Word(string from, string to)
            => Translates(from, to);

        [Theory]
        [InlineData("i.e.", "dvs.")]
        [InlineData("e.g.", "t.ex.")]
        public void Abbreviation(string from, string to)
            => Translates(from, to);

        [Theory]
        [InlineData("good-looking", "snygg")]
        [InlineData("non-English-speaking", "icke-engelsktalande")]
        [InlineData("non-achievement-oriented", "icke-prestationsorienterad")]
        public void HyphenedDoubleWord(string from, string to)
            => Translates(from, to);

        [Theory]
        [InlineData("easy as pie", "lätt som en pannkaka")]
        [InlineData("easy peasy", "lätt som en plätt")]
        [InlineData("will have been", "kommer att ha")]
        public void CompoundWordOrPhrase(string from, string to)
            => Translates(from, to);

        [Theory]
        [InlineData("Non-French-speaking", "Icke-fransktalande")]
        public void Capitalize(string from, string to)
            => Translates(from, to);

        [Theory]
        [InlineData("ball", "boll", "bolls", "bollen", "bollens", "bollar", "bollars", "bollarna", "bollarnas")]
        public void Generic(string stem, params string[] expected)
        {
            var candidates = Thesaurus.Translate(new Generic(stem))
                .Select(translation => translation.To)
                .ToArray();
            Assert.Equal(expected.OrderBy(v => v), candidates.OrderBy(v => v));
        }

        [Theory]
        [InlineData("search", "sök")]
        [InlineData("street", "gatu")]
        [InlineData("concert", "konsert")]
        public void IncompleteCompund(string from, string to)
        {
            var translations = Thesaurus.Translate(new Unclassified {Value = from});
            var incompleteCompound = translations.Single(translation => translation.IsIncompleteCompound);
            Assert.Equal(to, incompleteCompound.To);
        }

        private static void Translates(string from, string to)
        {
            var word = new Unclassified {Value = from.Split(' ')[0] };
            var translations = Thesaurus.Translate(word)
                .Where(t => t.From.Value == from)
                .Select(t => t.To);
            Assert.Contains(to, translations);
        }
    }
}