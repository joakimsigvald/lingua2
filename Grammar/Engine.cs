using System.Collections.Generic;

namespace Lingua.Grammar
{
    using Core;

    public class Engine : IGrammar
    {
        public (IReason Reason, IEnumerable<Translation> Translations) Reduce(
            IList<TreeNode<Translation>> possibilities)
            => Process.Execute(possibilities);
    }
}