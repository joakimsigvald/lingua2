using System.Collections.Generic;

namespace Lingua.Grammar
{
    using Core;
    public class GrammarEngine : IGrammar
    {
        private readonly IEvaluator _evaluator;
        public GrammarEngine(IEvaluator evaluator) => _evaluator = evaluator;

        public (Translation[] Translations, IReason Reason) Reduce(
            IList<Translation[]> possibilities)
            => Process.Execute(_evaluator, possibilities);

        public IEnumerable<Translation> Arrange(IEnumerable<Translation> translations)
            => _evaluator.Arrange(translations);
    }
}