using Lingua.Core;
using System;
using System.Linq;
using Xunit;

namespace Lingua.Grammar.Test
{
    public class ReverseCodeScoreNodeTests
    {
        [Theory]
        [InlineData("ATN", 1, "ATN:1")]
        [InlineData("^ATN", 1, "^ATN:1")]
        public void Test(string pattern, sbyte score, string expectedPatternLine)
        {
            var node = new ReverseCodeScoreNode();
            var reversedCode = Encoder.Encode(pattern).Reverse().ToArray();
            node.Extend(reversedCode, score);
            var patternLine = node.PatternLines.Single();
            Assert.Equal(expectedPatternLine, patternLine);
        }
    }
}