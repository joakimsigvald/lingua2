using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Adjective : Word
    {
        protected override Modifier GetAdditionalModifiers(int variationIndex)
            => GetIndividualModifiers(variationIndex)
                .Aggregate(Modifier.None, (first, second) => first | second);

        private static IEnumerable<Modifier> GetIndividualModifiers(int variationIndex)
        {
            if ((variationIndex & 1) > 0)
                yield return Modifier.Definite;
            if ((variationIndex & 2) > 0)
                yield return Modifier.Plural;
        }
    }
}