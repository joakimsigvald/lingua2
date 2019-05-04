using System.Collections.Generic;

namespace Lingua.Learning
{
    using Grammar;
    using Core.Extensions;
    using System.Linq;

    public class PatternCandidateProvider : IPatternCandidateProvider
    {
        private const int MaxAttempts = 128;

        private readonly Rearranger _arranger;
        private readonly List<TestCase> _testCases;
        private readonly PatternGenerator _patternGenerator;
        private readonly ITrainableEvaluator _evaluator;
        private ScoredPattern[] _scoredPatterns;
        private int _scoredPatternsIndex;
        private Arrangement[] _arrangementCandidates;
        private int _arrangementCandidatesIndex;
        private ScoredPattern? _currentScoredPattern;
        private Arrangement? _currentArranger;

        public PatternCandidateProvider(ITrainableEvaluator evaluator, List<TestCase> testCases, Rearranger arranger)
        {
            _patternGenerator = new PatternGenerator(new PatternExtractor());
            _evaluator = evaluator;
            _testCases = testCases;
            _arranger = arranger;
            _scoredPatterns = new ScoredPattern[0];
            _arrangementCandidates = new Arrangement[0];
        }

        public void ApplyNextPattern(TestSessionResult bestResult, TestCase failingTestCase)
        {
            _testCases.MoveToBeginning(failingTestCase);
            TryNextPattern(bestResult);
        }

        public void TryNextPattern(TestSessionResult bestResult)
        {
            while (!EnumerateNextPattern(bestResult))
                TryNextTarget(bestResult);
            AddNextPattern(bestResult);
        }

        public void GenerateNewPatterns(TestSessionResult result)
        {
            RenewScoredPatterns(result.FailedCase);
            RenewArrangementCandidates(result.FailedCase.TestCase);
        }

        public TestSessionResult UpdateFromResult(TestSessionResult? oldBestResult, TestSessionResult newResult)
        {
            if (oldBestResult >= newResult)
                return oldBestResult!;
            if (oldBestResult is null)
                GenerateNewPatterns(newResult);
            else
                PrepareToLearnNextPattern(oldBestResult!, newResult);
            return newResult;
        }

        private void PrepareToLearnNextPattern(TestSessionResult oldBestResult, TestSessionResult newBestResult)
        {
            if (newBestResult.SuccessCount > oldBestResult!.SuccessCount)
                PrepareToLearnNextTestCase(oldBestResult, newBestResult);
            else Reset();
            ClearCurrent();
        }

        private void ClearCurrent()
        {
            _currentScoredPattern = null;
            _currentArranger = null;
        }

        private void Reset()
        {
            if (_scoredPatternsIndex >= 0)
                _scoredPatternsIndex--;
        }

        private void PrepareToLearnNextTestCase(TestSessionResult oldBestResult, TestSessionResult newBestResult)
        {
            _testCases.MoveToBeginning(oldBestResult.FailedCase.TestCase);
            GenerateNewPatterns(newBestResult);
        }

        private bool EnumerateNextPattern(TestSessionResult bestResult)
        {
            if (bestResult.FailedCase.WordDeficit == 0)
            {
                if (_currentArranger != null)
                    RemoveArranger();
                if (++_arrangementCandidatesIndex < _arrangementCandidates.Length)
                    return true;
                _arrangementCandidatesIndex = -1;
            }
            if (_currentScoredPattern != null)
                RemoveScoredPattern();
            return ++_scoredPatternsIndex < _scoredPatterns.Length;
        }

        private void TryNextTarget(TestSessionResult bestResult)
        {
            bestResult.FailedCase.RemoveTarget();
            if (bestResult.FailedCase.TestCase.Target == null)
                throw new LearningFailed(bestResult);
            GenerateNewPatterns(bestResult);
        }

        private void RenewArrangementCandidates(TestCase testCase)
        {
            _arrangementCandidatesIndex = -1;
            _arrangementCandidates = ArrangerGenerator
                .GetArrangerCandidates(testCase.Target.Arrangement)
                .Except(_arranger.Arrangers)
                .ToArray();
        }

        private void RenewScoredPatterns(TestCaseResult result)
        {
            _scoredPatternsIndex = -1;
            _scoredPatterns = _patternGenerator
                .GetScoredPatterns(result)
                .Select(PrioritizePattern)
                .OrderBy(tuple => tuple.priority)
                .Take(MaxAttempts)
                .Select(tuple => tuple.sp)
                .ToArray();
        }

        private (ScoredPattern sp, int priority) PrioritizePattern(ScoredPattern sp)
            => (sp, ScoredPatternPriorityComputer.ComputePriority(_evaluator.GetScore(sp.ReversedCode), sp.Score, sp.ReversedCode));

        private void AddNextPattern(TestSessionResult bestResult)
        {
            if (bestResult!.FailedCase.WordDeficit == 0
                && _arrangementCandidatesIndex >= 0)
                AddArranger();
            else
                AddScoredPattern();
        }

        private void AddArranger()
        {
            _evaluator.Add(_currentArranger = _arrangementCandidates[_arrangementCandidatesIndex]);
            ResetResult();
        }

        private void RemoveArranger()
        {
            ResetResult();
            _evaluator.Remove(_currentArranger!);
            _currentArranger = null;
        }

        private void AddScoredPattern()
        {
            _evaluator.Do(_currentScoredPattern = _scoredPatterns[_scoredPatternsIndex]);
            ResetReductions();
        }

        private void RemoveScoredPattern()
        {
            ResetReductions();
            _evaluator.Undo(_currentScoredPattern!);
            _currentScoredPattern = null;
        }

        private void ResetReductions()
        {
            _testCases
                .Where(tc => tc.Reduction != null)
                .ForEach(tc =>
                {
                    tc.Reduction = null;
                    tc.Result = null;
                });
        }

        private void ResetResult()
        {
            _testCases
                .Where(tc => tc.Result != null)
                .ForEach(tc => tc.Result = null);
        }
    }
}