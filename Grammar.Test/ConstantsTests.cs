using System.Collections.Generic;
using Xunit;

namespace Lingua.Grammar.Test
{
    public class ConstantsTests
    {
        [Fact]
        public void CheckConstraints()
        {
            Assert.InRange(Constants.MaxArrangementLength, 0, Constants.Horizon);
            Assert.InRange(Constants.MaxPatternLength, 0, Constants.Horizon);
        }
    }
}