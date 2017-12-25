using System.Linq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class ScoredPatternPriorityComputerTests
    {
        [TestCase(4, 0, 1, 0)]
        [TestCase(4, 0, 1, 0x1000)]
        [TestCase(8, 0, -1, 0x1000)]
        [TestCase(1, -1, 1, 0x1000)]
        [TestCase(2, 1, -1, 0x1000)]
        [TestCase(2, -4, 1, 0x1000)]
        [TestCase(8, 3, 1, 0x1000)]
        [TestCase(1, 0, 1, 0x1400)] // wildcard
        [TestCase(5, 0, 1, 0x1001)] // 1 modifier
        [TestCase(7, 0, 1, 0x1011)] // 2 modifiers
        [TestCase(11, 0, 1, 0x1111)] // 3 modifiers
        [TestCase(10, 0, -1, 0x1000, 0x1400)]
        public void Test(int expectedPriority, int score, sbyte increment, params int[] code)
        {
            var actualPriority = ScoredPatternPriorityComputer.ComputePriority(score, increment, code.Select(i => (ushort)i).ToArray());
            Assert.That(actualPriority, Is.EqualTo(expectedPriority));
        }
    }
}
