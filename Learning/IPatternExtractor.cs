using System.Collections.Generic;

namespace Lingua.Learning
{
    public interface IPatternExtractor
    {
        IEnumerable<ushort[]> GetMatchingMonoCodes(ushort[] sequence);
        IEnumerable<ushort[]> GetMatchingCodes(ushort[] sequence, int length);
    }
}