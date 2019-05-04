using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public class Rearranger : IArranger
    {
        public IList<Arrangement> Arrangers = new List<Arrangement>();

        private static readonly Lazy<IList<Arrangement>> LoadedArrangers =
            new Lazy<IList<Arrangement>>(() => Repository.LoadArrangements().ToList());

        public void Load()
        {
            Arrangers = LoadedArrangers.Value;
        }

        public IGrammaton[] Arrange(IEnumerable<IGrammaton> grammatons)
            => DoArrange(grammatons.ToArray()).SelectMany(s => s).ToArray();

        public void Add(Arrangement arranger)
        {
            if (arranger != null && !Arrangers.Contains(arranger))
                Arrangers.Add(arranger);
        }

        public void Remove(Arrangement arranger)
        {
            Arrangers.Remove(arranger);
        }

        private IEnumerable<IEnumerable<IGrammaton>> DoArrange(IGrammaton[] grammatons)
        {
            for (var i = 0; i < grammatons.Length;)
            {
                var remaining = grammatons[i..];
                (var arrangedSegment, var length) = ArrangeSegment(remaining);
                i += Math.Max(1, length);
                yield return arrangedSegment ?? remaining.Take(1);
            }
        }

        private (IGrammaton[]? arrangement, int length) ArrangeSegment(IGrammaton[] remaining)
            => Arrangers
                .Select(arranger => Arrange(arranger, remaining))
                .FirstOrDefault(result => result.arrangement != null);

        private static (IGrammaton[]? arrangement, int length) Arrange(Arrangement arr, IEnumerable<IGrammaton> remaining)
        {
            var segment = remaining
                .Take(arr.Length)
                .ToArray();
            return arr.Arrange(segment);
        }
    }
}