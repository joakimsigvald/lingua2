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
        [Test]
        public void GivenNoExtractedPatterns_GenerateNoPatterns()
        {
            var patternGenerator = CreatePatternGenerator(new Translation[0][], new Translation[0]);
            var patterns = patternGenerator.GetMatchingPatterns(null);
            Assert.That(patterns, Is.Empty);
        }

        [TestCase("A")]
        [TestCase("A", "B")]
        public void GivenWantedExtractedMonoPatterns_GenerateThosePatternsWithScore_1(params string[] wantedMonoPatterns)
        {
            var patternGenerator = CreatePatternGenerator(new[] { new[] { new Translation() } }, new Translation[0], wantedMonoPatterns);
            var scoredPatterns = patternGenerator.GetMatchingPatterns(null);
            Assert.That(scoredPatterns.Count, Is.EqualTo(wantedMonoPatterns.Length));
            Assert.That(scoredPatterns.Select(sp => sp.Item1), Is.EquivalentTo(wantedMonoPatterns));
            Assert.That(scoredPatterns.All(sp => sp.Item2 == 1));
        }

        [TestCase("A")]
        [TestCase("A", "B")]
        public void GivenUnwantedExtractedMonoPatterns_GenerateThosePatternsWithScore_Minus_1(params string[] unwantedMonoPatterns)
        {
            var patternGenerator = CreatePatternGenerator(new Translation[0][], new[] { new Translation() }, unwantedMonoPatterns);
            var scoredPatterns = patternGenerator.GetMatchingPatterns(null);
            Assert.That(scoredPatterns.Count, Is.EqualTo(unwantedMonoPatterns.Length));
            Assert.That(scoredPatterns.Select(sp => sp.Item1), Is.EquivalentTo(unwantedMonoPatterns));
            Assert.That(scoredPatterns.All(sp => sp.Item2 == -1));
        }

        private static PatternGenerator CreatePatternGenerator(
            IEnumerable<Translation[]> wanted
            , IEnumerable<Translation> unwanted
            , params string[] monoPatterns)
        {
            var patternCandidateExtractorMock = new Mock<ITranslationExtractor>();
            patternCandidateExtractorMock.Setup(extractor => extractor.GetWantedTranslations(null))
                .Returns(wanted);
            patternCandidateExtractorMock.Setup(extractor => extractor.GetUnwantedTranslations(null))
                .Returns(unwanted);
            var patternExtractorMock = new Mock<IPatternExtractor>();
            patternExtractorMock.SetReturnsDefault(new string[0]);
            patternExtractorMock.Setup(extractor => extractor.GetMatchingMonoPatterns(It.Is<IEnumerable<Translation>>(v => v.Any())))
                .Returns(monoPatterns);
            return new PatternGenerator(patternCandidateExtractorMock.Object, patternExtractorMock.Object);
        }
    }
}