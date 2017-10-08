using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Pronoun : Word
    {
        public const ushort Code = 7 << Encoder.ModifierBits;

        protected override Modifier GetVariationModifier(int variationIndex)
        {
            switch (variationIndex)
            {
                case 1: return Modifier.Object;
                case 2: return Modifier.Possessive;
                case 3: return Modifier.Object | Modifier.Possessive;
                default: return Modifier.None;
            }
        }
    }
}