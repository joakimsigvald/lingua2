using System;
using System.Linq;

namespace Lingua.Learning
{
    public class LearningFailed : Exception
    {
        public TestSessionResult BestResult { get; }

        public LearningFailed(TestSessionResult bestResult)
            : base(string.Join("\n", CreateMessageLines(bestResult).ToArray()))
        {
            BestResult = bestResult;
        }

        private static string[] CreateMessageLines(TestSessionResult bestResult)
            => new[] {
                $"Failed testcase: '{bestResult.FailedCase.From}'",
                $"Expected: '{bestResult.FailedCase.Expected}'",
                $"Actual: '{bestResult.FailedCase.Actual}'"
            };
    }
}