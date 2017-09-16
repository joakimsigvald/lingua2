using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Noun : Word
    {
        protected override Modifier GetVariationModifier(int variationIndex)
        {
            switch (variationIndex)
            {
                case 1:
                    return Modifier.Definite;
                case 2:
                    return Modifier.Plural;
                case 3:
                    return Modifier.Definite | Modifier.Plural;
                default: return Modifier.None;
            }
        }
    }
}