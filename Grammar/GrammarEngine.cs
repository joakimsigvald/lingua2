using System.Collections.Generic;

namespace Lingua.Grammar
{
    using Core;
    public class GrammarEngine : IGrammar
    {
        private readonly IEvaluator _evaluator;
        public GrammarEngine(IEvaluator evaluator) => _evaluator = evaluator;

        public (ITranslation[] Translations, IReason Reason) Reduce(
            IList<ITranslation[]> possibilities)
            => Process.Execute(_evaluator, possibilities);

        public IEnumerable<ITranslation> Arrange(IEnumerable<ITranslation> translations)
            => _evaluator.Arrange(translations);
    }
}