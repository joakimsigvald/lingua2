using Xunit;

namespace Lingua.Core.Test
{
    public class CodeTests
    {
        [Theory]
        [InlineData("N", "N", false)]
        [InlineData("A", "N", true)]
        [InlineData("N", "A", false)]
        public void TestGreaterThan(string left, string right, bool expected)
        {
            Assert.Equal(expected, Encoder.Encode(left) > Encoder.Encode(right));
        }
    }
}