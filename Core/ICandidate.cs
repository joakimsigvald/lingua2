using System.Collections.Generic;

namespace Lingua.Core
{
    public interface ITranslationCandidate : IEnumerable<ITranslation>
    {
        ITranslation this[int index] { get; }
        int Length { get; }
    }

    public interface IGrammatonCandidate : IEnumerable<IGrammaton>
    {
        IGrammaton this[int index] { get; }
        int Length { get; }
    }
}