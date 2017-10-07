using System.Linq;
using Lingua.Core;
using NUnit.Framework;

namespace Lingua.Grammar.Test
{
    [TestFixture]
    public class ScorerTests
    {
        [TestCase("TN", 0)]
        [TestCase("TdqAdnNd", 1)]
        [TestCase("R1V1", 1)]
        [TestCase("R1nV1", 1)]
        [TestCase("R2V1", 1)]
        [TestCase("R2nV1", 1)]
        [TestCase("Vi", 1)]
        [TestCase("Vi", 1)]
        [TestCase("R3tX3R1mN", 1)]
        public void Score(string symbols, int expectedScore)
        {
            var tokens = Encoder.Deserialize(symbols).ToArray();
            var actualScore = Scorer.Evaluate(tokens).Score;
            Assert.That(actualScore, Is.EqualTo(expectedScore));
        }
    }
}