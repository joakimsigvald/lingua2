using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Article : Word
    {
        public const ushort Code = 5 << Encoder.ModifierBits;

        protected override Modifier GetVariationModifier(int variationIndex)
        {
            switch (variationIndex)
            {
                case 1:
                    return Modifier.Qualified;
                case 2:
                    return Modifier.Plural;
                case 3:
                    return Modifier.Qualified | Modifier.Plural;
                default: return Modifier.None;
            }
        }
    }
}