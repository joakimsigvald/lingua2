namespace Lingua.Core.Test
{
    public class TestCaseResult
    {
        public string From { get; set; }
        public string Expected { get; set; }
        public string Actual { get; set; }
        public bool Success { get; set; }
        public IReason Reason { get; set; }
    }
}