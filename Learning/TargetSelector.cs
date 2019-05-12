using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Extensions;
    using Grammar;
    using Lingua.Translation;

    public static class TargetSelector
    {
        private const int MaxTargets = 2;

        public static TranslationTarget[] SelectTargets(IDecomposition? decomposition, string translated)
        {
            if (decomposition is null)
                return new TranslationTarget[0];
            var orderedTranslations = SelectBestTranslationsWithOrder(decomposition!, translated)
                .Where((x, i) => i == 0 || x.order.Any())
                .Take(MaxTargets)
                .ToArray();
            if (!orderedTranslations.First().order.Any())
                throw new Exception($"Could not find possible translation for: {translated}, missing: {orderedTranslations.First().unmatched}");
            return orderedTranslations
                .Select(x => CreateTarget(x.translations, x.order))
                .ToArray();
        }

        private static TranslationTarget CreateTarget(ITranslation[] translations, byte[] order) 
            => new TranslationTarget(CreateArrangement(translations, order), translations);

        private static Arrangement CreateArrangement(IEnumerable<ITranslation> translations, byte[] order)
            => new Arrangement(translations.Select(t => t.Code).ToArray(), order);

        private static IEnumerable<(ITranslation[] translations, byte[] order, string unmatched)>
            SelectBestTranslationsWithOrder(IDecomposition decomposition, string translated)
        {
            var filteredDecomposition = Filter(decomposition, translated);
            var possibleSequences = new Expander(filteredDecomposition).Expand();
            return possibleSequences
                .Select(ps => new OrderMaker(translated).SelectAndOrderTranslations(ps))
                .OrderBy(o => o.unmatched.Length)
                .ThenBy(o => OutOfOrderCount(o.order))
                .ThenBy(o => o.hidden.Length)
                .Select(tuple => (tuple.translations, tuple.order, tuple.unmatched));
        }

        private static int OutOfOrderCount(byte[] order)
            => order.Any() 
            ? order.Skip(1).Select((n, i) => n != order[i] + 1).Prepend(order[0] != 0).Count(v => v)
            : 0;

        private static IDecomposition Filter(IDecomposition decomposition, string translated)
            => new Decomposition(decomposition.Select(ws => Filter(ws, translated)));

        private static IWordSpace Filter(IWordSpace wordSpace, string translated)
        {
            var matchningAlternatives = wordSpace
                .Where(cand => cand.All(t => translated.ContainsIgnoreCase(t.Output)))
                .OrderByDescending(cand => cand.Sum(t => t.Output.Length))
                .ToArray();
            var nonMatchingAlternative = wordSpace.Except(matchningAlternatives).Take(1);
            return new WordSpace(matchningAlternatives.Concat(nonMatchingAlternative));
        }
    }
}