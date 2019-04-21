using System;

namespace Lingua.Learning
{
    public class LearningFailed : Exception
    {
        public TestSessionResult BestResult { get; }

        public LearningFailed(TestSessionResult bestResult)
            : base($"Failed testcase: '{bestResult.FailedCase.From}'\n"
                  + $"Expected: '{bestResult.FailedCase.Expected}'\n"
                  + $"Actual: '{bestResult.FailedCase.Actual}'\n")
        {
            BestResult = bestResult;
        }
    }
}