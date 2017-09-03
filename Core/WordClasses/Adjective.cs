using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Adjective : Word
    {
        protected override IEnumerable<Modifier> GetIndividualModifiers(int variationIndex)
        {
            if ((variationIndex & 1) > 0)
                yield return Modifier.Definite;
            if ((variationIndex & 2) > 0)
                yield return Modifier.Plural;
        }
    }
}