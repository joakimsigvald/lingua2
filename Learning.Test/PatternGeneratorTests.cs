using System;
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
        private static readonly ushort[] WantedSequence = { 1 };
        private static readonly ushort[] UnwantedSequence = { 2 };

        [Test]
        public void GivenNoPatterns_GenerateNoScoredPatterns()
        {
            Assert.That(GetScoredPatterns(), Is.Empty);
        }

        [TestCase("A")]
        [TestCase("A", "C")]
        public void GivenWantedMonoPatterns_GenerateThosePatternsWithScore_1(params string[] wantedMonoPatterns)
        {
            var scoredPatterns = GetScoredPatterns(wantedMonoPatterns);
            Assert.That(scoredPatterns.Select(sp => sp.Pattern), Is.EquivalentTo(wantedMonoPatterns));
            Assert.That(scoredPatterns.All(sp => sp.Score == 1));
        }

        [TestCase("A")]
        [TestCase("A", "C")]
        public void GivenUnwantedMonoPatterns_GenerateThosePatternsWithScore_Minus_1(params string[] unwantedMonoPatterns)
        {
            var scoredPatterns = GetScoredPatterns(unwantedPatterns: unwantedMonoPatterns);
            Assert.That(scoredPatterns.Select(sp => sp.Pattern), Is.EquivalentTo(unwantedMonoPatterns));
            Assert.That(scoredPatterns.All(sp => sp.Score == -1));
        }

        [TestCase(new[] { "A" }, new[] { "C" })]
        [TestCase(new[] { "A", "C" }, new[] { "N" })]
        [TestCase(new[] { "A" }, new[] { "C", "N" })]
        [TestCase(new[] { "A" }, new[] { "A" })]
        [TestCase(new[] { "A", "C" }, new[] { "A", "N" })]
        public void GivenWantedAndUnwantedMonoPatterns_GenerateThosePatternsWithScore_PlusOrMinus_1(
            string[] wantedMonoPatterns
            , string[] unwantedMonoPatterns)
        {
            var scoredPatterns = GetScoredPatterns(wantedMonoPatterns, unwantedMonoPatterns);
            Assert.That(scoredPatterns
                .Where(sp => sp.Score == 1)
                .Select(sp => sp.Pattern), Is.EquivalentTo(wantedMonoPatterns));
            Assert.That(scoredPatterns
                .Where(sp => sp.Score == -1)
                .Select(sp => sp.Pattern), Is.EquivalentTo(unwantedMonoPatterns));
        }

        [TestCase("^A", "A")]
        [TestCase("^AA", "AA")]
        [TestCase("^A", "A", "^AA", "AA")]
        [TestCase("A", "AC", "ACN", "^ACN")]
        public void GivenMixedWantedPatterns_GenerateThosePatternsWithScore_1(params string[] patterns)
        {
            var scoredPatterns = GetScoredPatterns(patterns);
            Assert.That(scoredPatterns
                .Where(sp => sp.Score == 1)
                .Select(sp => sp.Pattern), Is.EquivalentTo(patterns));
        }

        [TestCase("^A", "A")]
        [TestCase("^AA", "AA")]
        [TestCase("^A", "A", "^AA", "AA")]
        [TestCase("A", "AC", "ACN", "^ACN")]
        public void GivenMixedUnwantedPatterns_GenerateThosePatternsWithScore_Minus_1(params string[] patterns)
        {
            var scoredPatterns = GetScoredPatterns(unwantedPatterns: patterns);
            Assert.That(scoredPatterns
                .Where(sp => sp.Score == -1)
                .Select(sp => sp.Pattern), Is.EquivalentTo(patterns));
        }

        private static IList<ScoredPattern> GetScoredPatterns(
            IReadOnlyCollection<string> wantedPatterns = null
            , IReadOnlyCollection<string> unwantedPatterns = null) 
            => CreatePatternGenerator(
                wantedPatterns ?? new string[0]
                , unwantedPatterns ?? new string[0])
            .GetScoredPatterns(null);

        private static PatternGenerator CreatePatternGenerator(
            IReadOnlyCollection<string> wantedPatterns
            , IReadOnlyCollection<string> unwantedPatterns) 
            => new PatternGenerator(MockTranslationExtractor(), MockPatternExtractor(wantedPatterns, unwantedPatterns));

        private static IPatternExtractor MockPatternExtractor(
            IReadOnlyCollection<string> wantedPatterns
            , IReadOnlyCollection<string> unwantedPatterns)
        {
            var patternExtractorMock = new Mock<IPatternExtractor>();
            patternExtractorMock.SetReturnsDefault(new string[0]);
            MockPatterns(patternExtractorMock, WantedSequence, MockWantedMultipatterns, wantedPatterns);
            MockPatterns(patternExtractorMock, UnwantedSequence, MockUnwantedMultipatterns, unwantedPatterns);
            return patternExtractorMock.Object;
        }

        private static void MockPatterns(
            Mock<IPatternExtractor> patternExtractorMock
            , ushort[] sequence
            , Action<Mock<IPatternExtractor>, IReadOnlyCollection<string>, int> mockMultiPatterns
            , IReadOnlyCollection<string> patterns)
        {
            var monopatterns = patterns.Where(p => p.Length == 1).ToArray();
            MockMonoPatterns(patternExtractorMock, sequence, monopatterns);
            var multipatterns = patterns.Except(monopatterns).ToArray();
            var length = 1;
            while (multipatterns.Any())
            {
                length++;
                var next = multipatterns.Where(p => p.Length == length).ToArray();
                multipatterns = multipatterns.Except(next).ToArray();
                mockMultiPatterns(patternExtractorMock, next, length);
            }
        }

        private static void MockMonoPatterns(
            Mock<IPatternExtractor> patternExtractorMock
            , ushort[] sequence
            , IEnumerable<string> patterns)
        {
            patternExtractorMock.Setup(extractor => extractor.GetMatchingMonoCodes(sequence))
                .Returns(patterns.Select(Encoder.Encode));
        }

        private static void MockWantedMultipatterns(
            Mock<IPatternExtractor> patternExtractorMock
            , IEnumerable<string> patterns
            , int length)
        {
            patternExtractorMock.Setup(extractor => extractor.GetMatchingCodes(
                WantedSequence, length))
                .Returns(patterns.Select(Encoder.Encode));
        }

        private static void MockUnwantedMultipatterns(
            Mock<IPatternExtractor> patternExtractorMock
            , IEnumerable<string> patterns
            , int length)
        {
            patternExtractorMock.Setup(extractor => extractor.GetMatchingCodes(
                UnwantedSequence, length))
                .Returns(patterns.Select(Encoder.Encode));
        }

        private static ITranslationExtractor MockTranslationExtractor()
        {
            var translationExtractorMock = new Mock<ITranslationExtractor>();
            translationExtractorMock.Setup(extractor => extractor.GetWantedSequence(null))
                .Returns(WantedSequence);
            translationExtractorMock.Setup(extractor => extractor.GetUnwantedSequence(null))
                .Returns(UnwantedSequence);
            return translationExtractorMock.Object;
        }
    }
}