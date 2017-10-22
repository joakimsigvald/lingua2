using System.Linq;
using Lingua.Core;
using Lingua.Testing;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class PatternGeneratorTests
    {
        [TestCase("N", "N*", "N", "^N*", "^N")]
        public void Test(string from, params string[] expected)
        {
            //given
            var testCase = new TestCase();
            var translationResult = new TranslationResult
            {
                Translations = new Translation[0]
            };
            var expectedCandidates = Encoder.Deserialize(from)
                .Select(token => new [] { new Translation {From = token}})
                .ToList();
            var testCaseResult = new TestCaseResult(testCase, translationResult, expectedCandidates);

            //when
            var matchingPatterns = PatternGenerator.GetMatchingPatterns(testCaseResult)
                .Select(sp => sp.Item1);

            //then
            Assert.That(matchingPatterns.Intersect(expected), Is.EquivalentTo(expected));
        }
    }
}