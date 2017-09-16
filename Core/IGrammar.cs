using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IGrammar
    {
        (IReason Reason, IEnumerable<Translation> Translations) Reduce(IList<TreeNode<Translation>> possibilities);
    }
}