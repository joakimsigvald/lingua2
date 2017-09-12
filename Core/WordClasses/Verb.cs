﻿using System.Collections.Generic;
using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Verb : Word
    {
        protected override IEnumerable<Modifier> GetIndividualModifiers(int variationIndex)
        {
            if (variationIndex > 0)
                yield return Modifier.FirstPerson;
            if (variationIndex == 3)
                yield return Modifier.SecondPerson;
            if (variationIndex > 3)
                yield return Modifier.Plural;
        }
    }
}