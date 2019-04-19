using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Article : Word
    {
        public const ushort Code = 5 << Encoder.ModifierCount;

        protected override Modifier GetVariationModifier(int variationIndex)
        {
            switch (variationIndex)
            {
                case 1:
                    return Modifier.Neuter;
                case 2:
                    return Modifier.Qualified;
                case 3:
                    return Modifier.Qualified | Modifier.Neuter;
                case 4:
                    return Modifier.Plural;
                case 5:
                    return Modifier.Qualified | Modifier.Plural;
                default: return Modifier.None;
            }
        }
    }
}