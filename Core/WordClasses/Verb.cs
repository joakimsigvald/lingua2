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
                    return Modifier.Plural;
                case 5:
                    return Modifier.ThirdPerson;
                case 6:
                    return Modifier.Past | Modifier.FirstPerson;
                case 7:
                    return Modifier.Past | Modifier.SecondPerson;
                case 9:
                    return Modifier.Past | Modifier.Plural;
                case 8:
                    return Modifier.Past | Modifier.ThirdPerson;
                case 10:
                    return Modifier.Perfect;
                case 11:
                    return Modifier.Continuous;
                case 12:
                    return Modifier.Continuous | Modifier.Past;
                case 13:
                    return Modifier.Continuous | Modifier.Perfect;
                case 14:
                    return Modifier.Continuous | Modifier.Future;
                case 15:
                    return Modifier.Passive;
                case 16:
                    return Modifier.Passive | Modifier.Past;
                case 17:
                    return Modifier.Passive | Modifier.Perfect;
                case 18:
                    return Modifier.Passive | Modifier.Continuous;
                case 19:
                    return Modifier.Passive | Modifier.Continuous | Modifier.Plural;
                default: return Modifier.None;
            }
        }
    }
}