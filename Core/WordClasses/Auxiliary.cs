using System.Collections.Generic;
using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Auxiliary : Word
    {
        protected override IEnumerable<Modifier> GetIndividualModifiers(int variationIndex)
        {
            if ((variationIndex & 1) > 0)
                yield return Modifier.FirstPerson;
            if ((variationIndex & 2) > 0)
                yield return Modifier.SecondPerson;
        }
    }
}