using System.Collections.Generic;
using Lingua.Core;

namespace Lingua.Vocabulary
{
    public interface ILexicon
    {
        IList<ITranslation> Lookup(string word);
        ITranslation[] PostApplyRules(ITranslation translation);
    }
}