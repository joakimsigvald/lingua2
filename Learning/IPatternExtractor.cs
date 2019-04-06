using Lingua.Core;
using System.Collections.Generic;

namespace Lingua.Learning
{
    public interface INewPatternExtractor
    {
        IEnumerable<Code> GetMatchingPatterns(Code code);
    }
}