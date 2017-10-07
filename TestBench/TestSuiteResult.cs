using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    public class TestSuiteResult
    {
        public string Caption { get; set; }
        public IList<TestCaseResult> Succeeded { get; set; }
        public IList<TestCaseResult> Failed { get; set; }
        public bool Success => !Failed.Any();
    }
}