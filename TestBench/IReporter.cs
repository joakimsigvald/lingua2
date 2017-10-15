using System.Collections.Generic;

namespace Lingua.Testing
{
    public interface IReporter
    {
        void Report(IEnumerable<TestCaseResult> testCaseResults);
    }
}