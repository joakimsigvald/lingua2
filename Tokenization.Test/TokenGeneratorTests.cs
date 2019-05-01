using Xunit;

namespace Lingua.Tokenization.Test
{
    public class TokenGeneratorTests
    {
        [Fact]
        public void WhenTextIsNull_GetZeroTokens()
        {
            var target = new TokenGenerator(new Tokenizer());
            var actual = target.GetTokens(null);
            Assert.Empty(actual);
        }
    }
}
