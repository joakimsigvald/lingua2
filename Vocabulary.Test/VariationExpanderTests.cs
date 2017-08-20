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
        public void Expand(string pattern, params string[] expected)
        {
            var actual = VariationExpander.Expand(pattern);
            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }
}
