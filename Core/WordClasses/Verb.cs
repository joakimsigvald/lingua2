using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Verb : Word
    {
        public const ushort Code = 10 << Encoder.ModifierBits;

        protected override Modifier GetVariationModifier(int variationIndex)
        {
            switch (variationIndex)
            {
                case 0:
                    return Modifier.Imperitive;
                case 2:
                    return Modifier.Participle;
                case 3:
                    return Modifier.FirstPerson;
                case 4:
                    return Modifier.SecondPerson;
                case 5:
                    return Modifier.ThirdPerson;
                case 6:
                    return Modifier.Plural | Modifier.ThirdPerson;
                case 7:
                    return Modifier.Past;
                case 8:
                    return Modifier.Perfect;
                case 9:
                    return Modifier.Participle | Modifier.Perfect;
                case 10:
                    return Modifier.Future;
                default: return Modifier.None;
            }
        }
    }
}