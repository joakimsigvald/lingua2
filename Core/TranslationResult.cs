namespace Lingua.Core
{
    public class TranslationResult
    {
        public string Translation { get; set; }
        public IReason Reason { get; set; }
        public Translation[] Translations { get; set; }
        public TranslationTreeNode Possibilities { get; set; }
    }
}