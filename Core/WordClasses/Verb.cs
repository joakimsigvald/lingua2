using System.Collections.Generic;
using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Verb : Word
    {
        protected override IEnumerable<Modifier> GetIndividualModifiers(int variationIndex)
        {
            if ((variationIndex & 1) > 0)
                yield return Modifier.Present;
        }
    }
}