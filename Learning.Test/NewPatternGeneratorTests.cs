﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;
using Moq;
using Xunit;
using static Xunit.Assert;

namespace Lingua.Learning.Test
{
    public class PatternGeneratorTests
    {
        [Fact]
        public void GivenNoResult_GenerateNoScoredPatterns()
        {
            Empty(CreateTarget().GetScoredPatterns(null));
        }

        [Theory]
        [InlineData("N", "V", "+N*", "+N", "+^N*", "+^N", "-V*", "-V", "-^V*", "-^V")]
        [InlineData("N", "N")]
        [InlineData("NA", "NV"
            , "+A*", "+A", "+N*A*", "+N*A", "+NA*", "+NA", "+^_A*", "+^N*A*", "+^_A", "+^N*A", "+^NA*", "+^NA"
            , "-V*", "-V", "-N*V*", "-N*V", "-NV*", "-NV", "-^_V*", "-^N*V*", "-^_V", "-^N*V", "-^NV*", "-^NV")]
        [InlineData("NA", "NN"
            , "+A*", "+A", "+N*A*", "+N*A", "+NA*", "+NA", "+^_A*", "+^N*A*", "+^_A", "+^N*A", "+^NA*", "+^NA"
            , "-N*", "-N", "-N*N*", "-N*N", "-NN*", "-NN", "-^_N*", "-^N*N*", "-^_N", "-^N*N", "-^NN*", "-^NN")]
        public void Test(string expectedPattern, string actualPattern, params string[] expectedScoredPatterns)
        {
            var patterns = GeneratePatterns(expectedPattern, actualPattern);

            Equal(expectedScoredPatterns.Length, patterns.Count);
            for (var i = 0; i < expectedScoredPatterns.Length; i++)
            {
                var (pattern, score) = ParseScoredPattern(expectedScoredPatterns[i]);
                Equal(pattern, patterns[i].Pattern);
                Equal(score, patterns[i].Score);
            }
        }

        private IList<ScoredPattern> GeneratePatterns(string expectedPattern, string actualPattern)
        {
            var generator = CreateTarget();
            var testCaseResult = MockTestCaseResult(expectedPattern, actualPattern);
            return generator.GetScoredPatterns(testCaseResult);
        }

        private PatternGenerator CreateTarget()
        {
            var extractor = new NewPatternExtractor();
            return new PatternGenerator(extractor);
        }

        private (string pattern, sbyte score) ParseScoredPattern(string str)
        {
            var score = (sbyte)(str[0] switch { '+' => 1, '-' => -1, _ => throw new NotImplementedException() });
            var partern = str.Substring(1);
            return (partern, score);
        }

        private ITestCaseResult MockTestCaseResult(string expectedPattern, string actualPattern)
        {
            var mock = new Mock<ITestCaseResult>();
            mock.Setup(res => res.ExpectedTranslations).Returns(ConvertToTranslations(expectedPattern));
            mock.Setup(res => res.ActualTranslations).Returns(ConvertToTranslations(actualPattern));
            return mock.Object;
        }

        private ITranslation[] ConvertToTranslations(string pattern) 
            => Encoder.Deserialize(pattern).Select(ConvertToTranslation).ToArray();

        private ITranslation ConvertToTranslation(Token token)
        {
            var mock = new Mock<ITranslation>();
            mock.Setup(t => t.Code).Returns(Encoder.Encode(token));
            return mock.Object;
        }
    }
}