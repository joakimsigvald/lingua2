using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class AdverbQualifying : Word
    {
        public const ushort Code = 16 << Encoder.ModifierCount;

        protected override Modifier GetVariationModifier(int variationIndex)
        {
            switch (variationIndex)
            {
                case 1:
                    return Modifier.Comparative;
                case 2:
                    return Modifier.Superlative;
                default: return Modifier.None;
            }
        }
    }
}