using NUnit.Framework;

namespace Lingua.Grammar.Test
{
    [TestFixture]
    public class ScorerTests
    {
        [TestCase("TN", 0)]
        public void Score(string serial, int expectedScore)
        {
            var tokens = Encoder.Deserialize(serial);
            var actualScore = Scorer.Compute(tokens);
            Assert.That(actualScore, Is.EqualTo(expectedScore));
        }
    }
}
