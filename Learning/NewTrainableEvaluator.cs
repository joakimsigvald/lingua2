using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Grammar;

    public class NewTrainableEvaluator : NewEvaluator, ITrainableEvaluator
    {
        private Rearranger _arranger;

        public NewTrainableEvaluator(Rearranger arranger) => _arranger = arranger;

        public void Do(ScoredPattern scoredPattern)
        {
            UpdateScore(scoredPattern.Code, scoredPattern.Score);
        }

        public void Undo(ScoredPattern scoredPattern)
        {
            UpdateScore(scoredPattern.Code, (sbyte) -scoredPattern.Score);
        }

        public void Add(Arranger arranger)
        {
            _arranger.Add(arranger);
        }

        public void Remove(Arranger arranger)
        {
            _arranger.Remove(arranger);
        }

        public int ComputeScoreDeficit(TestCaseResult failedCase)
        {
            var expected = Encoder.Encode(failedCase.ExpectedTranslations).ToArray();
            var actual = Encoder.Encode(failedCase.Translations).ToArray();
            var expectedScore = EvaluateReversed(expected.Reverse().ToArray());
            var actualScore = EvaluateReversed(actual.Reverse().ToArray());
            return actualScore - expectedScore;
        }

        public sbyte GetScore(ushort[] code)
            => GetScoreNode(Patterns, code.Reverse().ToArray(), 0)?.Score ?? 0;

        public void SavePatterns()
        {
            Repository.StoreScoredPatterns(Patterns.PatternLines);
            Repository.StoreRearrangements(_arranger.Arrangers);
        }

        public void UpdateScore(ushort[] code, sbyte addScore)
        {
            if (addScore == 0)
                return;
            UpdateScore(Patterns, code.Reverse().ToArray(), addScore, 0);
        }

        private static void UpdateScore(ReverseCodeScoreNode node, ushort[] reversedCode, sbyte score, int index)
        {
            if (reversedCode.Length == index)
                node.Score += score;
            else
            {
                var next = reversedCode[index];
                var child = node.Previous.FirstOrDefault(c => c.Code == next);
                if (child == null)
                {
                    child = new ReverseCodeScoreNode(next);
                    child.Extend(reversedCode.Skip(index + 1).ToArray(), score);
                    node.Previous.Add(child);
                    return;
                }
                UpdateScore(child, reversedCode, score, index + 1);
                if (child.Score == 0 && !child.Previous.Any())
                    node.Previous.Remove(child);
            }
        }

        private static ReverseCodeScoreNode? GetScoreNode(ReverseCodeScoreNode node, ushort[] reversedCode, int index)
        {
            if (reversedCode.Length == index)
                return node;
            var next = reversedCode[index];
            var child = node.Previous.FirstOrDefault(c => c.Code == next);
            return child == null ? null : GetScoreNode(child, reversedCode, index + 1);
        }
    }
}