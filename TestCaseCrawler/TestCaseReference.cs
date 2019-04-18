namespace TestCaseCrawler
{
    public struct TestCaseReference
    {
        public TestCaseReference(char letter, int number)
            => (Path, Tag) = ($"{letter}/{number}", $"{letter}-{number}");

        public string Path { get; }
        public string Tag { get; }
    }
}