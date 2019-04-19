using Xunit;

namespace Lingua.Vocabulary.Test
{
    public class VariationExpanderTests
    {
        [Theory]
        [InlineData("ball:s", "ball", "balls")]
        [InlineData("foot:___eet", "foot", "feet")]
        [InlineData("foot:_3eet", "foot", "feet")]
        [InlineData("ball::s", "ball", "ball", "balls")]
        [InlineData("boll:en:ar|na", "boll", "bollen", "bollar", "bollarna")]
        [InlineData("abc|d|e", "abc", "abcd", "abcde")]
        [InlineData("abc|_d:e|_f", "abc", "abd", "abce", "abcf")]
        [InlineData("the:::<d", "the", "the", "the", "the")]
        [InlineData("a!b!c", "a", "b", "c")]
        [InlineData("a:b!c:d", "a", "ab", "c", "ad")]
        [InlineData("a!b!c<d", "a", "b", "c")]
        [InlineData("a!b:c", "a", "b", "ac")]
        [InlineData("ball :s", "ball", "balls")]
        [InlineData("ball :  s", "ball", "balls")]
        [InlineData("ball 2:s", "ball", "balls", "balls")]
        [InlineData("a :b 2|c", "a", "ab", "abc", "abc")]
        [InlineData("ball 3!s", "ball", "s", "s", "s")]
        [InlineData("den:_+här", "den här", "de här")]
        public void ExpandVariants(string pattern, params string[] expected)
        {
            var actual = VariationExpander.Expand(pattern);
            Assert.Equal(expected, actual.Variations);
        }

        [Theory]
        [InlineData("sök/", "sök")]
        [InlineData("gata/_u", "gatu")]
        [InlineData("abc", null)]
        [InlineData("the:::<d", null)]
        [InlineData("a!b!c<d", null)]
        public void ExpandConnector(string pattern, string expected)
        {
            var actual = VariationExpander.Expand(pattern);
            Assert.Equal(expected, actual.IncompleteCompound);
        }

        [Theory]
        [InlineData("abc<def", "def")]
        [InlineData("the:::<d", "d")]
        [InlineData("a!b!c<d", "d")]
        public void ExpandModifiers(string pattern, string expected)
        {
            var actual = VariationExpander.Expand(pattern);
            Assert.Equal(expected, actual.Modifiers);
        }
    }
}
