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
            var patternCandidateExtractorMock = new Mock<ITranslationExtractor>();
            var patternExtractorMock = new Mock<IPatternExtractor>();
            patternExtractorMock.SetReturnsDefault(new string[0]);
            var patternGenerator = new PatternGenerator(patternCandidateExtractorMock.Object, patternExtractorMock.Object);
            var patterns = patternGenerator.GetMatchingPatterns(null);
            Assert.That(patterns, Is.Empty);
        }

        [Test]
        public void GivenOneWantedExtractedMonoPattern_GenerateThatPatternWithScore_1()
        {
            var expectedPatterns = new[] { "ABC" };
            var wanted = new []{new [] {new Translation()} };
            var unwanted = new Translation[0];

            var patternCandidateExtractorMock = new Mock<ITranslationExtractor>();
            patternCandidateExtractorMock.Setup(extractor => extractor.GetWantedTranslations(null))
                .Returns(wanted);
            patternCandidateExtractorMock.Setup(extractor => extractor.GetUnwantedTranslations(null))
                .Returns(unwanted);
            var patternExtractorMock = new Mock<IPatternExtractor>();
            patternExtractorMock.SetReturnsDefault(new string[0]);
            patternExtractorMock.Setup(extractor => extractor.GetMatchingMonoPatterns(It.Is<IEnumerable<Translation>>(v => v.Any())))
                .Returns(expectedPatterns);

            var patternGenerator = new PatternGenerator(patternCandidateExtractorMock.Object, patternExtractorMock.Object);
            var scoredPatterns = patternGenerator.GetMatchingPatterns(null);
            Assert.That(scoredPatterns.Count, Is.EqualTo(1));
            var scoredPattern = scoredPatterns.Single();
            Assert.That(scoredPattern.Item1, Is.EqualTo(expectedPatterns.Single()));
            Assert.That(scoredPattern.Item2, Is.EqualTo(1));
        }
    }
}