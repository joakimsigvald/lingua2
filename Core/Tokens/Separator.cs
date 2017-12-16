namespace Lingua.Core.Tokens
{
    public class Separator : Punctuation
    {
        public const ushort Code = 2 << Encoder.ModifierCount;

        public Separator(char character) : base(character)
        {
        }
    }
}