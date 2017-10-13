using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;
    public class Engine : IGrammar
    {
        private static readonly Dictionary<string, string> Rearrangements = Loader.LoadRearrangements();

        private static readonly IList<Arranger> Arrangers = Rearrangements.Select(sp => new Arranger(sp.Key, sp.Value)).ToList();

        public (IEnumerable<Translation> Translations, IReason Reason) Reduce(
            TreeNode<Tuple<Translation, ushort>> possibilities)
            => Process.Execute(possibilities);

        public IEnumerable<Translation> Arrange(IEnumerable<Translation> translations)
            => Arrangers
                .Aggregate(translations
                    , (input, arranger) => arranger
                        .Arrange(input.ToList()));
    }
}