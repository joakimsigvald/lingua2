using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Verb : Word
    {
        public const ushort Code = 10 << Encoder.ModifierCount;

        protected override Modifier GetVariationModifier(int variationIndex)
        {
            switch (variationIndex)
            {
                case 0:
                    return Modifier.Imperitive;
                case 2:
                    return Modifier.FirstPerson;
                case 3:
                    return Modifier.SecondPerson;
                case 4:
                    return Modifier.ThirdPerson;
                case 5:
                    return Modifier.Plural | Modifier.ThirdPerson;
                case 6:
                    return Modifier.Past | Modifier.FirstPerson;
                case 7:
                    return Modifier.Past | Modifier.SecondPerson;
                case 8:
                    return Modifier.Past | Modifier.ThirdPerson;
                case 9:
                    return Modifier.Past | Modifier.Plural | Modifier.ThirdPerson;
                case 10:
                    return Modifier.Perfect;
                case 11:
                    return Modifier.Continuous;
                case 12:
                    return Modifier.Past | Modifier.Continuous;
                case 13:
                    return Modifier.Perfect | Modifier.Continuous;
                case 14:
                    return Modifier.Future | Modifier.Continuous;
                default: return Modifier.None;
            }
        }
    }
}