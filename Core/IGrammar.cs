using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IGrammar
    {
        IEnumerable<Translation> Reduce(IList<TreeNode<Translation>> next);
    }
}