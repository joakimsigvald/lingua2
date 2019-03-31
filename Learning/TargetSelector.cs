using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Extensions;
    using Grammar;

    public static class TargetSelector
    {
        private const int MaxTargets = 2;

        public static TranslationTarget[] SelectTargets(IList<ITranslation[]> possibilities, string translated)
        {
            if (possibilities == null)
                return new TranslationTarget[0];
            var orderedTranslations = SelectBestTranslationsWithOrder(possibilities, translated);
            if (!orderedTranslations.First().order.Any())
                throw new Exception($"Could not find possible translation for: {translated}, missing: {orderedTranslations.First().unmatched}");
            return orderedTranslations
                .Where(x => x.order.Any())
                .Take(MaxTargets)
                .Select(x => CreateTarget(x.translations, x.order))
                .ToArray();
        }

        private static TranslationTarget CreateTarget(ITranslation[] translations, byte[] order) 
            => new TranslationTarget(CreateArrangement(translations, order), translations);

        private static Arrangement CreateArrangement(IEnumerable<ITranslation> translations, byte[] order)
            => new Arrangement(translations.Select(t => t.Code).ToArray(), order);

        private static (ITranslation[] translations, byte[] order, string unmatched, string hidden)[] SelectBestTranslationsWithOrder(IEnumerable<ITranslation[]> possibilities, string translated)
        {
            var filteredPossibilities = FilterPossibilities(possibilities, translated).ToList();
            var possibleSequences = new Expander(filteredPossibilities).Expand(out var _);
            return possibleSequences
                .Select(ps => new OrderMaker(translated).SelectAndOrderTranslations(ps))
                .OrderBy(o => o.unmatched.Length)
                .ThenBy(o => OutOfOrderCount(o.order))
                .ThenBy(o => o.hidden.Length)
                .ToArray();
        }

        private static int OutOfOrderCount(byte[] order)
            => order.Any() 
            ? order.Skip(1).Select((n, i) => n != order[i] + 1).Prepend(order[0] != 0).Count(v => v)
            : 0;

        private static IEnumerable<ITranslation[]> FilterPossibilities(
            IEnumerable<ITranslation[]> possibilities, string translated)
            => possibilities.Select(p => SelectAlternatives(p, translated));

        private static ITranslation[] SelectAlternatives(ITranslation[] alternatives, string translated)
        {
            var matchningAlternatives = alternatives.Where(t => translated.ContainsIgnoreCase(t.Output))
                .OrderByDescending(t => t.Output.Length);
            var nonMatchingAlternative = alternatives
                .Where(t => !translated.ContainsIgnoreCase(t.Output)).Take(1);
            return matchningAlternatives
                .Concat(nonMatchingAlternative)
                .ToArray();
        }
    }
}