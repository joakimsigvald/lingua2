using System.Linq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class ScoredPatternPriorityComputerTests
    {
        [TestCase(0, 0, 0)]
        [TestCase(4, 1, 0)]
        [TestCase(4, 1, 0x1000)]
        [TestCase(8, -1, 0x1000)]
        [TestCase(8, 4, 0x1000)]
        [TestCase(16, -4, 0x1000)]
        [TestCase(1, 1, 0x1400)] // wildcard
        [TestCase(5, 1, 0x1001)] // 1 modifier
        [TestCase(7, 1, 0x1011)] // 2 modifiers
        [TestCase(11, 1, 0x1111)] // 3 modifiers
        [TestCase(10, -1, 0x1000, 0x1400)]
        public void Test(int expectedPriority, int score, params int[] code)
        {
            var actualPriority = ScoredPatternPriorityComputer.ComputePriority(score, code.Select(i => (ushort)i).ToArray());
            Assert.That(actualPriority, Is.EqualTo(expectedPriority));
        }
    }
}
