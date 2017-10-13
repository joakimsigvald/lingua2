using Lingua.Core;

namespace Lingua.Testing
{
    public class TestCaseResult
    {
        public string Group { get; set; }
        public string From { get; set; }
        public string Expected { get; set; }
        public string Actual { get; set; }
        public bool Success { get; set; }
        public IReason Reason { get; set; }
    }
}