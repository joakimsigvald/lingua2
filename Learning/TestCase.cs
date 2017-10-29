namespace Lingua.Learning
{
    public class TestCase
    {
        public TestCase(string from, string expected)
        {
            From = from;
            Expected = expected;
        }

        public string Suite { get; set; }
        public string From { get; set; }
        public string Expected { get; set; }
    }
}