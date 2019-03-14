using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using static Lingua.TestUtils.Assert;

namespace Lingua.Learning.Test
{
    using Core;

    public class PatternGeneratorTests
    {
        private static readonly ushort[] WantedSequence = {1};
        private static readonly ushort[] UnwantedSequence = {2};

        [Fact]
        public void GivenNoPatterns_GenerateNoScoredPatterns()
        {
            Assert.Empty(GetScoredPatterns());
        }

        [Theory]
        [InlineData("A")]
        [InlineData("A", "C")]
        public void GivenWantedMonoPatterns_GenerateThosePatternsWithScore_1(params string[] wantedMonoPatterns)
        {
            var scoredPatterns = GetScoredPatterns(wantedMonoPatterns);
            Assert.Equal(wantedMonoPatterns, scoredPatterns.Select(sp => sp.Pattern));
            Assert.True(scoredPatterns.All(sp => sp.Score == 1));
        }

        [Theory]
        [InlineData("A")]
        [InlineData("A", "C")]
        public void GivenUnwantedMonoPatterns_GenerateThosePatternsWithScore_Minus_1(
            params string[] unwantedMonoPatterns)
        {
            var scoredPatterns = GetScoredPatterns(unwantedPatterns: unwantedMonoPatterns);
            Assert.Equal(unwantedMonoPatterns, scoredPatterns.Select(sp => sp.Pattern));
            Assert.True(scoredPatterns.All(sp => sp.Score == -1));
        }

        [Theory]
        [InlineData("A", "^A")]
        [InlineData("AA", "^AA")]
        [InlineData("A", "^A", "AA", "^AA")]
        [InlineData("A", "AC", "ACN", "^ACN")]
        public void GivenMixedWantedPatterns_GenerateThosePatternsWithScore_1(params string[] patterns)
        {
            var scoredPatterns = GetScoredPatterns(patterns);
            Assert.Equal(patterns, scoredPatterns
                .Where(sp => sp.Score == 1)
                .Select(sp => sp.Pattern));
        }

        [Theory]
        [InlineData("A", "^A")]
        [InlineData("AA", "^AA")]
        [InlineData("A", "^A", "AA", "^AA")]
        [InlineData("A", "AC", "ACN", "^ACN")]
        public void GivenMixedUnwantedPatterns_GenerateThosePatternsWithScore_Minus_1(params string[] expectedPatterns)
        {
            var scoredPatterns = GetScoredPatterns(unwantedPatterns: expectedPatterns);
            Assert.Equal(expectedPatterns, scoredPatterns
                .Where(sp => sp.Score == -1)
                .Select(sp => sp.Pattern));
        }

        [Theory]
        [InlineData("N", "V", "+N*", "+N", "-V*", "-V", "+^N*", "+^N", "-^V*", "-^V")]
        [InlineData("NV", "VN"
            , "+N*V*", "+NV*", "+N*V", "+NV", "+^N*", "+^N", "+^_V*", "+^_V", "+^N*V*", "+^NV*", "+^N*V", "+^NV"
            , "-V*N*", "-VN*", "-V*N", "-VN", "-^V*", "-^V", "-^_N*", "-^_N", "-^V*N*", "-^VN*", "-^V*N", "-^VN")]
        public void TestAllPatterns(string wantedSequence, string unwantedSequence, params string[] scoredPatterns)
        {
            var translationExtractorMock = new Mock<ITranslationExtractor>();
            translationExtractorMock.Setup(extractor => extractor.GetWantedSequence(null))
                .Returns(Encoder.Encode(wantedSequence));
            translationExtractorMock.Setup(extractor => extractor.GetUnwantedSequence(null))
                .Returns(Encoder.Encode(unwantedSequence));
            var patternExtractor = new PatternExtractor();
            var generator = new PatternGenerator(translationExtractorMock.Object, patternExtractor);

            var actualPatterns = generator.GetScoredPatterns(null);

            var expectedPatterns = scoredPatterns.Select(CreateScoredPattern).ToHashSet();
            Equivalent(expectedPatterns, actualPatterns);
        }

        [Theory]
        [InlineData("NPV", "VNP"
            , "+PV", "+N_V", "+NPV", "+^N", "+^_P", "+^NP", "+^__V", "+^_PV", "+^N_V", "+^NPV"
            , "-VN", "-V_P", "-VNP", "-^V", "-^VN", "-^_N", "-^__P", "-^_NP", "-^V_P", "-^VNP")]
        [InlineData("NN", "N"
            , "+N", "+NN", "+^_N", "+^NN")]
        [InlineData("N", "NN"
            , "-N", "-NN", "-^_N", "-^NN")]
        public void TestUngeneralizedPatterns(string wantedSequence, string unwantedSequence, params string[] scoredPatterns)
        {
            var translationExtractorMock = new Mock<ITranslationExtractor>();
            translationExtractorMock.Setup(extractor => extractor.GetWantedSequence(null))
                .Returns(Encoder.Encode(wantedSequence));
            translationExtractorMock.Setup(extractor => extractor.GetUnwantedSequence(null))
                .Returns(Encoder.Encode(unwantedSequence));
            var patternExtractor = new PatternExtractor();
            var generator = new PatternGenerator(translationExtractorMock.Object, patternExtractor);

            var actualUngeneralizedPatterns = generator
                .GetScoredPatterns(null)
                .Where(p => !IsGeneralized(p))
                .ToHashSet();

            var expectedUngeneralizedPatterns = scoredPatterns
                .Select(CreateScoredPattern)
                .Where(p => !IsGeneralized(p))
                .ToHashSet();
            Equivalent(expectedUngeneralizedPatterns, actualUngeneralizedPatterns);
        }

        private static bool IsGeneralized(ScoredPattern scoredPattern)
            => scoredPattern.Pattern.Contains('*');

        private static ScoredPattern CreateScoredPattern(string str)
        {
            var score = str[0] == '+' ? 1 : -1;
            var code = Encoder.Encode(str.Substring(1));
            return new ScoredPattern(code, (sbyte)score);
        }

        private static IList<ScoredPattern> GetScoredPatterns(
            IReadOnlyCollection<string>? wantedPatterns = null
            , IReadOnlyCollection<string>? unwantedPatterns = null) 
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