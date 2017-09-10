using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Adjective : Word
    {
        protected override IEnumerable<Modifier> GetIndividualModifiers(int variationIndex)
        {
            if (variationIndex == 1)
                yield return Modifier.Plural;
            if (variationIndex == 1 || variationIndex == 4)
                yield return Modifier.Definite;
            if (variationIndex == 2)
                yield return Modifier.Comparative;
            if (variationIndex > 2)
                yield return Modifier.Superlative;
        }
    }
}