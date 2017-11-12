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
            var scoredPatterns = GetScoredPatterns(unwantedPatterns: unwantedMonoPatterns);
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

        [TestCase(new[] { "A" }, new[] { "A" })]
        [TestCase(new[] { "A", "B" }, new[] { "A", "C" })]
        public void GivenOverlappingWantedAndUnwantedMonoPatterns_GenerateNonOverlappingPatternsWithScore_PlusOrMinus_1(
            string[] wantedMonoPatterns
            , string[] unwantedMonoPatterns)
        {
            var distinctPatterns = wantedMonoPatterns.Except(unwantedMonoPatterns)
                .Concat(unwantedMonoPatterns.Except(wantedMonoPatterns))
                .Distinct()
                .ToArray();
            var scoredPatterns = GetScoredPatterns(wantedMonoPatterns, unwantedMonoPatterns);
            Assert.That(scoredPatterns
                .Select(sp => sp.Item1), Is.EquivalentTo(distinctPatterns));
        }

        [TestCase("^A", "A")]
        [TestCase("^AA", "AA")]
        [TestCase("^A", "A", "^AA", "AA")]
        [TestCase("A", "AB", "ABC", "^ABC")]
        public void GivenMixedWantedPatterns_GenerateThosePatternsWithScore_1(params string[] wantedPatterns)
        {
            var scoredPatterns = GetScoredPatterns(wantedPatterns);
            Assert.That(scoredPatterns.Select(sp => sp.Item1), Is.EquivalentTo(wantedPatterns));
            Assert.That(scoredPatterns.All(sp => sp.Item2 == 1));
        }

        private static IList<(string, sbyte)> GetScoredPatterns(
            IReadOnlyCollection<string> wantedPatterns = null
            , IReadOnlyCollection<string> unwantedPatterns = null) 
            => CreatePatternGenerator(
                wantedPatterns ?? new string[0]
                , unwantedPatterns ?? new string[0])
            .GetMatchingPatterns(null);

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
            MockPatterns(patternExtractorMock, WantedTranslation, wantedPatterns);
            MockPatterns(patternExtractorMock, UnwantedTranslation, unwantedPatterns);
            return patternExtractorMock.Object;
        }

        private static void MockPatterns(
            Mock<IPatternExtractor> patternExtractorMock
            , Translation translation
            , IReadOnlyCollection<string> patterns)
        {
            var monopatterns = patterns.Where(p => p.Length == 1).ToArray();
            MockMonoPatterns(patternExtractorMock, translation, monopatterns);
            var multipatterns = patterns.Except(monopatterns).ToArray();
            var length = 1;
            while (multipatterns.Any())
            {
                length++;
                var next = multipatterns.Where(p => p.Length == length).ToArray();
                multipatterns = multipatterns.Except(next).ToArray();
                MockMultipatterns(patternExtractorMock, translation, next, length);
            }
        }

        private static void MockMonoPatterns(
            Mock<IPatternExtractor> patternExtractorMock
            , Translation translation
            , IEnumerable<string> patterns)
        {
            patternExtractorMock.Setup(extractor => extractor.GetMatchingMonoPatterns(
                    It.Is<IEnumerable<Translation>>(v => v.Contains(translation))))
                .Returns(patterns);
        }

        private static void MockMultipatterns(
            Mock<IPatternExtractor> patternExtractorMock
            , Translation translation
            , IEnumerable<string> patterns
            , int length)
        {
            patternExtractorMock.Setup(extractor => extractor.GetMatchingPatterns(
                    It.Is<ICollection<Translation[]>>(v => v.Single().Contains(translation)), length))
                .Returns(patterns);
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