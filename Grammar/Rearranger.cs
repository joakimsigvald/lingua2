using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public class Rearranger : IArranger
    {
        public IList<Arranger> Arrangers = new List<Arranger>();

        private static readonly Lazy<IList<Arranger>> LoadedArrangers =
            new Lazy<IList<Arranger>>(() => BuildArrangers(Repository.LoadArrangements()));

        public void Load()
        {
            Arrangers = LoadedArrangers.Value;
        }

        public ITranslation[] Arrange(IEnumerable<ITranslation> translations)
            => DoArrange(translations.ToArray()).SelectMany(s => s).ToArray();

        public void Add(Arranger arranger)
        {
            if (arranger != null && !Arrangers.Contains(arranger))
                Arrangers.Add(arranger);
        }

        public void Remove(Arranger arranger)
        {
            Arrangers.Remove(arranger);
        }

        private IEnumerable<IEnumerable<ITranslation>> DoArrange(ICollection<ITranslation> translations)
        {
            for (var i = 0; i < translations.Count;)
            {
                var remaining = translations.Skip(i).ToArray();
                (var arrangedSegment, var length) = ArrangeSegment(remaining);
                i += Math.Max(1, length);
                yield return arrangedSegment ?? remaining.Take(1);
            }
        }

        private (ITranslation[] arrangement, int length) ArrangeSegment(IList<ITranslation> remainingTranslations)
            => Arrangers
                .Select(arranger => Arrange(arranger, remainingTranslations))
                .FirstOrDefault(result => result.arrangement != null);

        private static (ITranslation[] arrangement, int length) Arrange(Arranger arr, IEnumerable<ITranslation> remainingTranslations)
        {
            var segment = remainingTranslations
                .Take(arr.Length)
                .ToArray();
            return arr.Arrange(segment);
        }

        private static IList<Arranger> BuildArrangers(IEnumerable<Arrangement> arrangements)
            => arrangements.Select(arr => new Arranger(arr)).ToList();
    }
}