using Lingua.Core.Extensions;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    [TestFixture]
    public class IntExtensionsTests
    {
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        [TestCase(3, 2)]
        [TestCase(5, 2)]
        [TestCase(21, 3)]
        public void CountBits(int number, byte count)
        {
            Assert.That(number.CountBits(), Is.EqualTo(count));
        }
    }
}
