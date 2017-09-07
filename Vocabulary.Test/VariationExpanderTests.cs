using NUnit.Framework;

namespace Lingua.Vocabulary.Test
{
    [TestFixture]
    public class VariationExpanderTests
    {
        [TestCase("ball:s", "ball", "balls")]
        [TestCase("foot:___eet", "foot", "feet")]
        [TestCase("ball::s", "ball", "ball", "balls")]
        [TestCase("boll:en:ar|na", "boll", "bollen", "bollar", "bollarna")]
        [TestCase("abc|d|e", "abc", "abcd", "abcde")]
        [TestCase("abc|_d:e|_f", "abc", "abd", "abce", "abcf")]
        [TestCase("the:::<d", "the", "the", "the", "the")]
        public void ExpandVariants(string pattern, params string[] expected)
        {
            var actual = VariationExpander.Expand(pattern);
            Assert.That(actual.Variations, Is.EquivalentTo(expected));
        }

        [TestCase("sök/", "sök")]
        [TestCase("gata/_u", "gatu")]
        [TestCase("abc", null)]
        [TestCase("the:::<d", null)]
        public void ExpandConnector(string pattern, string expected)
        {
            var actual = VariationExpander.Expand(pattern);
            Assert.That(actual.IncompleteCompound, Is.EqualTo(expected));
        }

        [TestCase("abc<def", "def")]
        [TestCase("the:::<d", "d")]
        public void ExpandModifiers(string pattern, string expected)
        {
            var actual = VariationExpander.Expand(pattern);
            Assert.That(actual.Modifiers, Is.EqualTo(expected));
        }
    }
}
