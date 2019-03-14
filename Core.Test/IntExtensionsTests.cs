using Lingua.Core.Extensions;
using Xunit;

namespace Lingua.Core.Test
{
    public class IntExtensionsTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(3, 2)]
        [InlineData(5, 2)]
        [InlineData(21, 3)]
        public void CountBits(int number, byte expected)
        {
            Assert.Equal(expected, number.CountBits());
        }
    }
}
