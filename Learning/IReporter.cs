using System.Collections.Generic;

namespace Lingua.Learning
{
    public interface IReporter
    {
        void Report(IEnumerable<TestCaseResult> testCaseResults);
    }
}