using System.Collections.Generic;
using System.Linq;
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

        [TestCase("1", "1")]
        [TestCase("1,2", "12")]
        [TestCase("12", "1,2")]
        [TestCase("12,3", "13,23")]
        [TestCase("12,34", "13,14,23,24")]
        public void Combine(string setsStr, string combinationsStr)
        {
            var sets = ExtractLists(setsStr);// new[] { new [] {1}};
            var expectedCombinations = ExtractLists(combinationsStr);// new[] { new[] { 1 } };
            var actualCombinations = sets.Expand();
            Assert.That(actualCombinations, Is.EquivalentTo(expectedCombinations));
        }

        private static IEnumerable<IEnumerable<int>> ExtractLists(string str) 
            => str.Split(',')
            .Where(s => s.Any())
            .Select(s => s.Select(c => c - 48));
    }
}
