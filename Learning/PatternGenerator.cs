using Lingua.Core;
using Lingua.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    public class PatternGenerator
    {
        public const byte MaxPatternLength = 6;

        private readonly INewPatternExtractor _patternExtractor;
        private CodeCondenser _codeCondenser = new CodeCondenser();

        public PatternGenerator(INewPatternExtractor patternExtractor)
        {
            _patternExtractor = patternExtractor;
        }

        public IList<ScoredPattern> GetScoredPatterns(ITestCaseResult? result)
        {
            if (result == null)
                return new ScoredPattern[0];
            var wantedCodes = GetPossibleCodesInOrder(result.ExpectedTranslations);
            var unwantedCodes = GetPossibleCodesInOrder(result.ActualTranslations);
            FilterOutCommonCodes(ref wantedCodes, ref unwantedCodes);
            var patterns =
                GetScoredPatterns(wantedCodes.Distinct().ToList(), 1)
                .Concat(GetScoredPatterns(unwantedCodes.Distinct().ToList(), -1))
                .ToArray();
            var uniqueScoredPatterns = GetMajorityScoreOfEachUniquePattern(patterns);
            return uniqueScoredPatterns;
        }

        private IList<ScoredPattern> GetMajorityScoreOfEachUniquePattern(IEnumerable<ScoredPattern> patterns)
            => patterns
                .GroupBy(p => p.Code)
                .Where(g => g.Sum(sp => sp.Score) != 0)
                .Select(g => g.OrderBy(sp => sp.Score).Skip(g.Count() / 2).First())
                .ToArray();

        public List<Code> GetPossibleCodesInOrder(IEnumerable<ITranslation> translations)
            => _codeCondenser
            .GetReversedCodes(translations)
            .Select(rc => new Code(rc))
            .SelectMany(GetAllSubCodes)
            .OrderBy(sc => sc)
            .ToList();

        private IEnumerable<Code> GetAllSubCodes(Code code)
            => Enumerable
            .Range(1, Math.Min(code.Length, MaxPatternLength))
            .Select(i => code.Take(i));

        private void FilterOutCommonCodes(ref List<Code> leftInOrder, ref List<Code> rightInOrder)
        {
            var li = 0;
            var ri = 0;
            while (li < leftInOrder.Count && ri < rightInOrder.Count)
            {
                var lc = leftInOrder[li];
                var rc = rightInOrder[ri];
                if (lc < rc)
                    li++;
                else if (rc < lc)
                    ri++;
                else
                {
                    leftInOrder.RemoveAt(li);
                    rightInOrder.RemoveAt(ri);
                }
            }
        }

        private IEnumerable<ScoredPattern> GetScoredPatterns(IList<Code> codes, sbyte score)
            => PatternLengths
                .SelectMany(
                length => codes
                .Where(c => c.Length == length)
                .SelectMany(_patternExtractor.GetMatchingPatterns))
                .Select(snippet => new ScoredPattern(snippet, score));

        private static IEnumerable<byte> PatternLengths
            => Enumerable.Range(1, MaxPatternLength).Select(n => (byte)n);
    }
}