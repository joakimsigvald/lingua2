namespace Lingua.Learning
{
    public interface ITranslationExtractor
    {
        ushort[] GetWantedSequence(TestCaseResult result);
        ushort[] GetUnwantedSequence(TestCaseResult result);
    }
}