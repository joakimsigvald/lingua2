using System;

namespace Lingua.Learning
{
    public class LearningFailed : Exception
    {
        public TestSessionResult BestResult { get; }

        public LearningFailed(TestSessionResult bestResult)
        {
            BestResult = bestResult;
        }
    }
}