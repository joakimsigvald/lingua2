using System;
using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Verb : Word
    {
        protected override Modifier GetVariationModifier(int variationIndex)
        {
            switch (variationIndex)
            {
                case 1:
                    return Modifier.Definite; // Participle
                case 2:
                    return Modifier.FirstPerson;
                case 3:
                    return Modifier.SecondPerson;
                case 4:
                    return Modifier.ThirdPerson;
                case 5:
                    return Modifier.Plural | Modifier.ThirdPerson;
                case 6:
                    return Modifier.Past;
                default: return Modifier.None;
            }
        }
    }
}