using System.Linq;
using Lingua.Core.Tokens;
using Xunit;

namespace Lingua.Learning.Test
{
    public class ScoredPatternPriorityComputerTests
    {
        [Theory]
        [InlineData(4, 0, 1, (ushort)0)]
        [InlineData(4, 0, 1, (ushort)0x1000)]
        [InlineData(8, 0, -1, (ushort)0x1000)]
        [InlineData(1, -1, 1, (ushort)0x1000)]
        [InlineData(2, 1, -1, (ushort)0x1000)]
        [InlineData(2, -4, 1, (ushort)0x1000)]
        [InlineData(8, 3, 1, (ushort)0x1000)]
        [InlineData(2, 0, 1, (ushort)0x1400)] // wildcard
        [InlineData(5, 0, 1, (ushort)0x1001)] // 1 modifier
        [InlineData(7, 0, 1, (ushort)0x1011)] // 2 modifiers
        [InlineData(11, 0, 1, (ushort)0x1111)] // 3 modifiers
        [InlineData(12, 0, -1, (ushort)0x1000, (ushort)0x1400)]
        [InlineData(1, 0, 1, AnyToken.Code)]
        [InlineData(0, -1, 1, AnyToken.Code)]
        public void Test(int expectedPriority, int score, int increment, params ushort[] code)
        {
            var actualPriority = ScoredPatternPriorityComputer.ComputePriority(score, (sbyte)increment, code);
            Assert.Equal(expectedPriority, actualPriority);
        }
    }
}
