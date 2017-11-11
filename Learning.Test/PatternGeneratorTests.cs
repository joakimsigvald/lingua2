using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    using Core;

    [TestFixture]
    public class PatternGeneratorTests
    {
        private static readonly Translation WantedTranslation = new Translation();
        private static readonly Translation UnwantedTranslation = new Translation();

        [Test]
        public void GivenNoPatterns_GenerateNoScoredPatterns()
        {
            Assert.That(GetScoredPatterns(), Is.Empty);
        }

        [TestCase("A")]
        [TestCase("A", "B")]
        public void GivenWantedMonoPatterns_GenerateThosePatternsWithScore_1(params string[] wantedMonoPatterns)
        {
            var scoredPatterns = GetScoredPatterns(wantedMonoPatterns);
            Assert.That(scoredPatterns.Select(sp => sp.Item1), Is.EquivalentTo(wantedMonoPatterns));
            Assert.That(scoredPatterns.All(sp => sp.Item2 == 1));
        }

        [TestCase("A")]
        [TestCase("A", "B")]
        public void GivenUnwantedMonoPatterns_GenerateThosePatternsWithScore_Minus_1(params string[] unwantedMonoPatterns)
        {
            var scoredPatterns = GetScoredPatterns(unwantedMonoPatterns: unwantedMonoPatterns);
            Assert.That(scoredPatterns.Select(sp => sp.Item1), Is.EquivalentTo(unwantedMonoPatterns));
            Assert.That(scoredPatterns.All(sp => sp.Item2 == -1));
        }

        [TestCase(new[] { "A" }, new[] { "B" })]
        [TestCase(new[] { "A", "B" }, new[] { "C" })]
        [TestCase(new[] { "A" }, new[] { "B", "C" })]
        public void GivenDifferentWantedAndUnwantedMonoPatterns_GenerateThosePatternsWithScore_PlusOrMinus_1(
            string[] wantedMonoPatterns
            , string[] unwantedMonoPatterns)
        {
            var scoredPatterns = GetScoredPatterns(wantedMonoPatterns, unwantedMonoPatterns);
            Assert.That(scoredPatterns
                .Where(sp => sp.Item2 == 1)
                .Select(sp => sp.Item1), Is.EquivalentTo(wantedMonoPatterns));
            Assert.That(scoredPatterns
                .Where(sp => sp.Item2 == -1)
                .Select(sp => sp.Item1), Is.EquivalentTo(unwantedMonoPatterns));
        }

        private static IList<(string, sbyte)> GetScoredPatterns(
            IEnumerable<string> wantedMonoPatterns = null
            , IEnumerable<string> unwantedMonoPatterns = null) 
            => CreatePatternGenerator(wantedMonoPatterns, unwantedMonoPatterns)
            .GetMatchingPatterns(null);

        private static PatternGenerator CreatePatternGenerator(
            IEnumerable<string> wantedMonoPatterns = null
            , IEnumerable<string> unwantedMonoPatterns = null) 
            => new PatternGenerator(MockTranslationExtractor(), MockPatternExtractor(wantedMonoPatterns, unwantedMonoPatterns));

        private static IPatternExtractor MockPatternExtractor(
            IEnumerable<string> wantedMonoPatterns = null
            , IEnumerable<string> unwantedMonoPatterns = null)
        {
            var patternExtractorMock = new Mock<IPatternExtractor>();
            patternExtractorMock.SetReturnsDefault(new string[0]);
            patternExtractorMock.Setup(extractor => extractor.GetMatchingMonoPatterns(
                    It.Is<IEnumerable<Translation>>(v => v.Contains(WantedTranslation))))
                .Returns(wantedMonoPatterns);
            patternExtractorMock.Setup(extractor => extractor.GetMatchingMonoPatterns(
                    It.Is<IEnumerable<Translation>>(v => v.Contains(UnwantedTranslation))))
                .Returns(unwantedMonoPatterns);
            return patternExtractorMock.Object;
        }

        private static ITranslationExtractor MockTranslationExtractor()
        {
            var translationExtractorMock = new Mock<ITranslationExtractor>();
            translationExtractorMock.Setup(extractor => extractor.GetWantedTranslations(null))
                .Returns(new[] { new[] { WantedTranslation } });
            translationExtractorMock.Setup(extractor => extractor.GetUnwantedTranslations(null))
                .Returns(new[] { UnwantedTranslation });
            return translationExtractorMock.Object;
        }
    }
}