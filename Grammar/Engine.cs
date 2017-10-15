using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;
    public class Engine : IGrammar
    {
        private readonly IEvaluator _evaluator;
        private static readonly Dictionary<string, string> Rearrangements = Loader.LoadRearrangements();

        private static readonly IList<Arranger> Arrangers = Rearrangements.Select(sp => new Arranger(sp.Key, sp.Value)).ToList();

        public Engine(IEvaluator evaluator) => _evaluator = evaluator;

        public (Translation[] Translations, IReason Reason) Reduce(
            TranslationTreeNode possibilities)
            => Process.Execute(_evaluator, possibilities);

        public Translation[] Arrange(IEnumerable<Translation> translations)
            => Arrangers
                .Aggregate(translations
                    , (input, arranger) => arranger
                        .Arrange(input.ToList()))
            .ToArray();
    }
}