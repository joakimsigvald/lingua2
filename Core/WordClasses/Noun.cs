using System.Collections.Generic;
using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Noun : Word
    {
        protected override IEnumerable<Modifier> GetIndividualModifiers(int variationIndex)
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