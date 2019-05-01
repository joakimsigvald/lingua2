using Lingua.Grammar;
namespace Lingua.Learning
{
    public interface IPatternCandidateProvider
    {
        void TryNextPattern(TestSessionResult bestResult);
        void GenerateNewPatterns(TestSessionResult result);
        void PrepareToLearnNextPattern(TestSessionResult oldBestResult, TestSessionResult newBestResult);
        TestSessionResult UpdateFromResult(TestSessionResult? oldBestResult, TestSessionResult newResult);
        void ApplyNextPattern(TestSessionResult bestResult, TestCase failingTestCase);
    }
}