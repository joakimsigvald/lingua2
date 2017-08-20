using System.Collections.Generic;
using Lingua.Core;

namespace Lingua.Vocabulary
{
    public interface ILexicon
    {
        IList<Translation> Lookup(string word);
    }
}