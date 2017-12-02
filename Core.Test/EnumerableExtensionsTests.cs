using Lingua.Core.Extensions;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [TestCase(true, new int[] { })]
        [TestCase(true, new[] { 1 }, 1)]
        [TestCase(true, new[] { 1, 2 }, 1, 2)]
        [TestCase(true, new int[] { }, 1, 2)]
        [TestCase(true, new[] { 1 }, 1, 2)]
        [TestCase(true, new[] { 2 }, 1, 2)]
        [TestCase(true, new[] { 2, 1 }, 3, 2, 1)]
        [TestCase(false, new[] { 1, 2 }, 3, 2, 1)]
        [TestCase(false, new[] { 4, 3 }, 3, 2, 1)]
        [TestCase(false, new[] { 3, 1 }, 3, 2, 1)]
        public void IsSegmentOf(bool expected, int[] a, params int[] b)
        {
            Assert.That(a.IsSegmentOf(b), Is.EqualTo(expected));
        }
    }
}
