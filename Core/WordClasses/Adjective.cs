using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Adjective : Word
    {
        protected override Modifier GetVariationModifier(int variationIndex)
        {
            switch (variationIndex)
            {
                case 1:
                    return Modifier.Definite | Modifier.Plural;
                case 2:
                    return Modifier.Comparative;
                case 3:
                    return Modifier.Superlative;
                case 4:
                    return Modifier.Superlative | Modifier.Definite;
                default: return Modifier.None;
            }
        }
    }
}