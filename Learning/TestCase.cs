namespace Lingua.Learning
{
    public class TestCase
    {
        public TestCase(string from, string expected)
        {
            From = from;
            Expected = expected;
        }

        public TranslationTarget Target { get; set; }
        public string Suite { get; set; }
        public string From { get; }
        public string Expected { get; }
    }
}