using System.Collections.Generic;
using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Verb : Word
    {
        protected override IEnumerable<Modifier> GetIndividualModifiers(int variationIndex)
        {
            if (variationIndex == 1)
                yield return Modifier.Definite; // Participle
            if (variationIndex > 1)
                yield return Modifier.FirstPerson;
            if (variationIndex > 3)
                yield return Modifier.SecondPerson;
            if (variationIndex == 5)
                yield return Modifier.Plural;
        }
    }
}