using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Moq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class PatternGeneratorTests
    {
        [Test]
        public void GivenNoExtractedPatterns_GenerateNoPatterns()
        {
            var patternExtractorMock = new Mock<IPatternExtractor>();
            patternExtractorMock.SetReturnsDefault(new string[0]);
            var patternGenerator = new PatternGenerator(patternExtractorMock.Object, Mock.Of<ITranslationExtractor>());
            var patterns = patternGenerator.GetMatchingPatterns(null);
            Assert.That(patterns, Is.Empty);
        }

        [Test]
        public void GivenOneWantedExtractedMonoPattern_GenerateThatPatternWithScore_1()
        {
            var expectedPatterns = new[] { "ABC" };
            IEnumerable<Translation[]> wanted = new [] { new [] { new Translation()} };

            var patternExtractorMock = new Mock<IPatternExtractor>();
            patternExtractorMock.SetReturnsDefault(new string[0]);
            patternExtractorMock.Setup(extractor => extractor.GetMatchingMonoPatterns(It.Is<IEnumerable<Translation>>(x => x.Any())))
                .Returns(expectedPatterns);

            var translationExtractorMock = new Mock<ITranslationExtractor>();
            patternExtractorMock.SetReturnsDefault(new string[0]);
            translationExtractorMock.Setup(extractor => extractor.GetWantedTranslations(It.IsAny<TestCaseResult>()))
                .Returns(wanted);

            var patternGenerator = new PatternGenerator(patternExtractorMock.Object, translationExtractorMock.Object);
            var scoredPatterns = patternGenerator.GetMatchingPatterns(null);
            Assert.That(scoredPatterns.Count, Is.EqualTo(1));
            var scoredPattern = scoredPatterns.Single();
            Assert.That(scoredPattern.Item1, Is.EqualTo(expectedPatterns.Single()));
            Assert.That(scoredPattern.Item2, Is.EqualTo(1));
        }
    }
}