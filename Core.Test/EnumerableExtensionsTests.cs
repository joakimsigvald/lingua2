using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Extensions;
using Xunit;

namespace Lingua.Core.Test
{
    public class EnumerableExtensionsTests
    {
        [Theory]
        [InlineData(true, new int[] { })]
        [InlineData(true, new[] { 1 }, 1)]
        [InlineData(true, new[] { 1, 2 }, 1, 2)]
        [InlineData(true, new int[] { }, 1, 2)]
        [InlineData(true, new[] { 1 }, 1, 2)]
        [InlineData(true, new[] { 2 }, 1, 2)]
        [InlineData(true, new[] { 2, 1 }, 3, 2, 1)]
        [InlineData(false, new[] { 1, 2 }, 3, 2, 1)]
        [InlineData(false, new[] { 4, 3 }, 3, 2, 1)]
        [InlineData(false, new[] { 3, 1 }, 3, 2, 1)]
        public void IsSegmentOf(bool expected, int[] a, params int[] b)
        {
            Assert.Equal(expected, a.IsSegmentOf(b));
        }

        [Theory]
        [InlineData("1", "1")]
        [InlineData("1,2", "12")]
        [InlineData("12", "1,2")]
        [InlineData("12,3", "13,23")]
        [InlineData("12,34", "13,14,23,24")]
        public void Combine(string setsStr, string combinationsStr)
        {
            var sets = ExtractLists(setsStr);// new[] { new [] {1}};
            var expectedCombinations = ExtractLists(combinationsStr);// new[] { new[] { 1 } };
            var actualCombinations = sets.Expand();
            Assert.Equal(expectedCombinations, actualCombinations);
        }

        private static IEnumerable<IEnumerable<int>> ExtractLists(string str) 
            => str.Split(',')
            .Where(s => s.Any())
            .Select(s => s.Select(c => c - 48));
    }
}
