using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Adjective : Word
    {
        public const ushort Code = 8 << Encoder.ModifierCount;

        protected override Modifier GetVariationModifier(int variationIndex)
        {
            switch (variationIndex)
            {
                case 1:
                    return Modifier.Neuter;
                case 2:
                    return Modifier.Definite | Modifier.Plural;
                case 3:
                    return Modifier.Comparative;
                case 4:
                    return Modifier.Superlative;
                case 5:
                    return Modifier.Superlative | Modifier.Definite;
                case 6:
                    return Modifier.Adverb;
                default: return Modifier.None;
            }
        }
    }
}