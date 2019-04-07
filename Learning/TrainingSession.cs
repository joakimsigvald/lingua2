using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Extensions;
    using Core.Tokens;
    using Grammar;
    using TestCaseTranslators;

    internal class TrainingSession
    {
        //private const int MaxAttempts = 256;
        private const int MaxAttempts = 3256;

        private readonly ITrainableEvaluator _evaluator;
        private readonly Rearranger _arranger;
        private readonly Translator _translator;
        private readonly List<TestCase> _testCases;
        private readonly TestRunner _testRunner;
        private readonly PatternGenerator _patternGenerator;

        private IList<ScoredPattern> _scoredPatternsList;
        private IEnumerator<ScoredPattern> _scoredPatterns;
        private IList<Arranger> _arrangementCandidatesList;
        private IEnumerator<Arranger> _arrangementCandidates;
        private TestSessionResult _bestResult;
        private ScoredPattern _currentScoredPattern;
        private Arranger _currentArranger;

        public TrainingSession(ITrainableEvaluator evaluator, Rearranger arranger, Translator translator, IEnumerable<TestCase> testCases)
        {
            _evaluator = evaluator;
            _arranger = arranger;
            _translator = translator;
            _testCases = PrepareForLearning(testCases);
            _testRunner = CreateLearningTestRunner();
            _patternGenerator = new PatternGenerator(new NewPatternExtractor());
        }

        private TestRunner CreateLearningTestRunner()
        {
            var settings = new TestRunnerSettings
            {
                AbortOnFail = true,
                AllowReordered = false
            };
            return new TestRunner(new PossibitiesTranslator(_translator), _evaluator, settings);
        }

        private List<TestCase> PrepareForLearning(IEnumerable<TestCase> testCases)
        {
            var preparedTestCases = testCases
                .Where(tc => !string.IsNullOrWhiteSpace(tc.From))
                .ToList();
            preparedTestCases.ForEach(tc =>  tc.PrepareForLearning(_translator));
            var prioritizedTestCases = preparedTestCases
                .Select(tc => (testcase: tc, priority: ComputePriority(tc.Target.Translations)))
                .OrderBy(pair => pair.priority);
            return prioritizedTestCases
                .Select(pair => pair.testcase)
                .ToList();
        }

        private static int ComputePriority(IReadOnlyCollection<ITranslation> translations)
            => translations.Count
               + translations.Select(t => t.From.GetType()).Distinct().Count()
               + translations.Select(t => t.From)
                   .OfType<Element>()
                   .Select(e => e.Modifiers)
                   .Distinct().Count();

        public TestSessionResult LearnPatterns()
        {
            TestSessionResult result;
            while (!(result = _testRunner.RunTestSession(_testCases)).Success)
            {
                if (result.SuccessCount < (_bestResult?.SuccessCount ?? 0))
                {
                    _testCases.MoveToBeginning(result.FailedCase.TestCase);
                    TryNextPattern();
                    continue;
                }
                if (_bestResult == null)
                    GenerateNewPatterns(_bestResult = result);
                else if (result > _bestResult)
                    PrepareToLearnNextPattern(result);
                TryLearnTestCase();
            }
            return result;
        }

        private void PrepareToLearnNextPattern(TestSessionResult result)
        {
            _currentScoredPattern = null;
            _currentArranger = null;
            if (result.SuccessCount > _bestResult.SuccessCount)
                PrepareToLearnNextTestCase(result);
            else ResetPatterns();
            _bestResult = result;
        }

        private void ResetPatterns()
        {
            _scoredPatterns.Reset();
            _arrangementCandidates.Reset();
        }

        private void PrepareToLearnNextTestCase(TestSessionResult result)
        {
            _testCases.MoveToBeginning(_bestResult.FailedCase.TestCase);
            _scoredPatterns?.Dispose();
            _arrangementCandidates?.Dispose();
            GenerateNewPatterns(result);
        }

        private void TryLearnTestCase()
        {
            TestCaseResult result;
            do
            {
                TryNextPattern();
                //TODO: Optimize by using different learning-translator for training arrangers
                result = _testRunner.RunTestCase(_bestResult.FailedCase.TestCase);
            } while (!(result.Success || result > _bestResult.FailedCase));
        }

        private void TryNextPattern()
        {
            while (!EnumerateNextPattern())
                TryNextTarget();
            AddNextPattern();
        }

        private bool EnumerateNextPattern()
        {
            if (_bestResult.FailedCase.WordDeficit == 0)
            {
                if (_currentArranger != null)
                    RemoveArranger();
                _currentArranger = null;
                if (_arrangementCandidates.MoveNext())
                    return true;
                _arrangementCandidates.Reset();
            }
            if (_currentScoredPattern != null)
                RemoveScoredPattern();
            _currentScoredPattern = null;
            return _scoredPatterns.MoveNext();
        }

        private void AddNextPattern()
        {
            if (_bestResult.FailedCase.WordDeficit == 0
                && _arrangementCandidates.Current != null)
                AddArranger();
            else
                AddScoredPattern();
        }

        private void AddScoredPattern()
        {
            _evaluator.Do(_currentScoredPattern = _scoredPatterns.Current);
            ResetReductions();
        }

        private void RemoveScoredPattern()
        {
            ResetReductions();
            _evaluator.Undo(_currentScoredPattern);
        }

        private void AddArranger()
        {
            _evaluator.Add(_currentArranger = _arrangementCandidates.Current);
            ResetResult();
        }

        private void RemoveArranger()
        {
            ResetResult();
            _evaluator.Remove(_currentArranger);
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

        private static IEnumerable<ushort[]> SkipToLastCode(IEnumerable<ushort[]> possibilities, ushort[] pattern, int offset)
            => offset == pattern.Length
                ? possibilities
                : SkipToLastCode(possibilities.SkipWhile(c => !c.Any(t => Encoder.Matches(t, pattern[offset]))), pattern, offset + 1);

        private static IEnumerable<ushort> SkipToLastCode(IEnumerable<ushort> translations, ushort[] pattern, int offset)
            => offset == pattern.Length
                ? translations
                : SkipToLastCode(translations.SkipWhile(t => !Encoder.Matches(t, pattern[offset])), pattern, offset + 1);

        private void TryNextTarget()
        {
            _scoredPatterns?.Dispose();
            _arrangementCandidates?.Dispose();
            _bestResult.FailedCase.RemoveTarget();
            if (_bestResult.FailedCase.TestCase.Target == null)
                throw new LearningFailed(_bestResult);
            GenerateNewPatterns(_bestResult);
        }

        private void GenerateNewPatterns(TestSessionResult result)
        {
            RenewScoredPatterns(result.FailedCase);
            RenewArrangementCandidates(result.FailedCase.TestCase);
        }

        private void RenewScoredPatterns(TestCaseResult result)
        {
            _scoredPatternsList = _patternGenerator
                .GetScoredPatterns(result)
                .Select(PrioritizePattern)
                .OrderBy(tuple => tuple.priority)
                .Take(MaxAttempts)
                .Select(tuple => tuple.sp)
                .ToList();
            _scoredPatterns = _scoredPatternsList.GetEnumerator();
        }

        private (ScoredPattern sp, int priority) PrioritizePattern(ScoredPattern sp)
            => (sp, ScoredPatternPriorityComputer.ComputePriority(_evaluator.GetScore(sp.ReversedCode), sp.Score, sp.ReversedCode));

        private void RenewArrangementCandidates(TestCase testCase)
        {
            _arrangementCandidatesList = ArrangerGenerator.GetArrangerCandidates(testCase.Target.Arrangement)
                .Except(_arranger.Arrangers)
                .ToList();
            _arrangementCandidates = _arrangementCandidatesList.GetEnumerator();
        }
    }
}