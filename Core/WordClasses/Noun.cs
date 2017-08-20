using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Noun : Word
    {
        protected override Modifier GetModifiers(int variationIndex)
            => GetIndividualModifiers(variationIndex)
            .Aggregate(Modifier.None, (first, second) => first | second);

        private static IEnumerable<Modifier> GetIndividualModifiers(int variationIndex)
        {
            if ((variationIndex & 1) > 0)
                yield return Modifier.Possessive;
            if ((variationIndex & 2) > 0)
                yield return Modifier.Definite;
            if ((variationIndex & 4) > 0)
                yield return Modifier.Plural;
        }
    }
}