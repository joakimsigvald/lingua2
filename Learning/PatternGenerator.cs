using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    public class PatternGenerator
    {
        private readonly ITranslationExtractor _translationExtractor;
        private readonly IPatternExtractor _patternExtractor;

        public PatternGenerator(ITranslationExtractor translationExtractor, IPatternExtractor patternExtractor)
        {
            _translationExtractor = translationExtractor;
            _patternExtractor = patternExtractor;
        }

        public IList<ScoredPattern> GetScoredPatterns(TestCaseResult result)
        {
            var wantedCode = _translationExtractor.GetWantedSequence(result);
            var unwantedCode = _translationExtractor.GetUnwantedSequence(result);
            var scoredMonoPatterns = GetScoredMonoPatterns(wantedCode, unwantedCode);
            var scoredultiPatterns = GetScoredMultiPatterns(wantedCode, unwantedCode);
            return scoredMonoPatterns.Concat(scoredultiPatterns).ToArray();
        }

        private IEnumerable<ScoredPattern> GetScoredMonoPatterns(
            ushort[] wanted
            , ushort[] unwanted)
            => _patternExtractor
                .GetMatchingMonoCodes(wanted)
                .Select(code => new ScoredPattern(code, 1, 1))
                .Concat(_patternExtractor.GetMatchingMonoCodes(unwanted)
                    .Select(code => new ScoredPattern(code, 1, -1)));

        private IEnumerable<ScoredPattern> GetScoredMultiPatterns(
            ushort[] wanted
            , ushort[] unwanted)
            => MultiPatternLengths
                .SelectMany(length => GetScoredMultiPatterns(wanted, unwanted, length));

        private static IEnumerable<byte> MultiPatternLengths 
            => Enumerable.Range(2, 5).Select(n => (byte)n);

        private IEnumerable<ScoredPattern> GetScoredMultiPatterns(
            ushort[] wanted
            , ushort[] unwanted
            , byte length)
            => GetScoredMultiPatterns(wanted, length, 1)
                .Concat(GetScoredMultiPatterns(unwanted, length, -1));

        private IEnumerable<ScoredPattern> GetScoredMultiPatterns(
            ushort[] code
            , byte length, sbyte score)
            => _patternExtractor.GetMatchingCodes(code, length)
                .Select(snippet => new ScoredPattern(snippet, length, score));
    }
}